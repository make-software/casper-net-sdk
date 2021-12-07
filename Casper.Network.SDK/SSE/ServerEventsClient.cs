using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    [Flags]
    public enum ChannelType
    {
        Main,
        Deploys,
        Sigs
    };

    [Flags]
    public enum EventType
    {
        All,
        ApiVersion,
        BlockAdded,
        DeployAccepted,
        DeployProcessed,
        Fault,
        Step,
        FinalitySignature,
        DeployExpired
    }

    internal struct EventData
    {
        public bool HasData => !string.IsNullOrEmpty(Payload);

        public EventType EventType { get; set; }
        public int Id { get; set; }
        public string Payload { get; set; }
    }

    public class ServerEventsClient
    {
        private readonly Dictionary<EventType, ChannelType> _evt2Channel;
        private Dictionary<ChannelType, int> _channels;

        private readonly List<SSECallback> _callbacks;

        private readonly string _host;
        private readonly int _port;
        private readonly Dictionary<ChannelType, Tuple<Task, CancellationTokenSource>> _runningTasks;

        public ServerEventsClient(string host, int port)
        {
            _host = host;
            _port = port;
            _callbacks = new List<SSECallback>();

            _evt2Channel = new Dictionary<EventType, ChannelType>()
            {
                {EventType.DeployAccepted, ChannelType.Deploys},
                {EventType.BlockAdded, ChannelType.Main},
                {EventType.DeployProcessed, ChannelType.Main},
                {EventType.Fault, ChannelType.Main},
                {EventType.Step, ChannelType.Main},
                {EventType.FinalitySignature, ChannelType.Sigs},
                {EventType.DeployExpired, ChannelType.Main}
            };
            _runningTasks = new Dictionary<ChannelType, Tuple<Task, CancellationTokenSource>>();
        }

        public void AddEventCallback(EventType eventType, string name, EventCallback cb, int startFrom = int.MaxValue)
        {
            var callback = new SSECallback(eventType, name, cb);
            if (_callbacks.Contains(callback))
                throw new ArgumentException($"A callback for '{callback}' already exist. Remove it first.",
                    nameof(name));

            _callbacks.Add(callback);

            UpdateChannels(eventType, startFrom);
        }

        public bool RemoveEventCallback(EventType eventType, string name)
        {
            var found = _callbacks.Remove(new SSECallback(eventType, name));

            UpdateChannels(eventType);

            return found;
        }

        private void UpdateChannels(EventType eventType, int startFrom = int.MaxValue)
        {
            var ch = new Dictionary<ChannelType, int>();
            foreach (var cb in _callbacks)
            {
                if (cb.EventType == EventType.All)
                {
                    // cant handle startFrom for EventType.All, skipping parameter
                    _channels = new Dictionary<ChannelType, int>()
                    {
                        {ChannelType.Main, int.MaxValue}, {ChannelType.Deploys, int.MaxValue},
                        {ChannelType.Sigs, int.MaxValue}
                    };
                    return;
                }

                if (_evt2Channel.ContainsKey(cb.EventType))
                {
                    var channel = _evt2Channel[cb.EventType];
                    if (ch.ContainsKey(channel) && channel == _evt2Channel[eventType])
                        ch[channel] = Math.Min(ch[channel], startFrom);
                    else
                        ch.Add(_evt2Channel[cb.EventType], startFrom);
                }
            }

            _channels = ch;
        }

        public void StartListening()
        {
            if (_channels.Count == 0)
                throw new Exception("No channels to listen. Add callbacks first.");
            if (_runningTasks.Count > 0)
                return;

            foreach (var channelType in _channels)
            {
                if (!_runningTasks.ContainsKey(channelType.Key))
                {
                    var tokenSource = new CancellationTokenSource();
                    var task = ListenChannelAsync(channelType.Key, channelType.Value, tokenSource.Token);
                    _runningTasks.Add(channelType.Key, new Tuple<Task, CancellationTokenSource>(task, tokenSource));
                    Thread.Sleep(3000);
                }
            }
        }

        public async Task StopListening()
        {
            var tasks = new List<Task>();

            foreach (var runningTask in _runningTasks)
            {
                runningTask.Value.Item2.Cancel();
                tasks.Add(runningTask.Value.Item1);
            }

            await Task.WhenAll(tasks);
        }

        public void Wait()
        {
            // returns when all channel listeners are closed
            //
            var tasks = new List<Task>();

            foreach (var runningTask in _runningTasks)
                tasks.Add(runningTask.Value.Item1);

            Task.WhenAll(tasks).Wait();
        }

        private Task ListenChannelAsync(ChannelType channelType, int? startFrom, CancellationToken cancelToken)
        {
            var task = Task.Run(async () =>
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var eventData = new EventData();

                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var uriBuilder = new UriBuilder("http", _host, _port,
                            $"events/{channelType.ToString().ToLower()}");

                        if (startFrom != null && startFrom != int.MaxValue)
                            uriBuilder.Query = $"start_from={startFrom}";
                        else
                            uriBuilder.Query = $"start_from={0}";
                            
                        using (var streamReader =
                            new StreamReader(await client.GetStreamAsync(uriBuilder.Uri, cancelToken)))
                        {
                            while (!streamReader.EndOfStream && !cancelToken.IsCancellationRequested)
                            {
                                var message = await streamReader.ReadLineAsync();
                         
                                if (ParseStream(message, ref eventData))
                                {
                                    EmitEvent(eventData);
                                    eventData = new EventData();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.WriteLine("Retrying in 5 seconds");
                        Thread.Sleep(5000);
                    }
                }
            }, cancelToken);

            return task;
        }

        private static bool ParseStream(string line, ref EventData eventData)
        {
            if (string.IsNullOrEmpty(line) || line.Trim().Equals(":"))
                return eventData.HasData
                    ;
            if (line.TrimStart().StartsWith(@"data:{""ApiVersion"""))
            {
                eventData.EventType = EventType.ApiVersion;
                eventData.Id = 0;
                eventData.Payload = line.Trim().Substring(5);
                return true;
            }

            if (line.Trim().StartsWith("data:{"))
            {
                // extract event type from first json object
                var q1 = line.IndexOf('"');
                var q2 = line.IndexOf('"', q1 + 1);
                var evtType = line.Substring(q1 + 1, q2 - q1 - 1);
                if (Enum.TryParse(evtType, out EventType evt))
                {
                    eventData.EventType = evt;
                    eventData.Id = 0;
                    eventData.Payload = line.Trim().Substring(5);
                    ;
                }

                // id needed to complete the event
                return false;
            }

            if (line.Trim().StartsWith("id:"))
            {
                if (int.TryParse(line.Substring(3).Trim(), out var id))
                {
                    eventData.Id = id;
                    return true;
                }

                return false;
            }

            return false;
        }

        private void EmitEvent(EventData eventData)
        {
            JsonDocument jsonDoc = null;

            foreach (var callback in _callbacks)
            {
                try
                {
                    if (callback.EventType == EventType.All || callback.EventType == eventData.EventType)
                    {
                        if (jsonDoc == null)
                            jsonDoc = JsonDocument.Parse(eventData.Payload);

                        callback.CallbackFn(new SSEvent()
                        {
                            EventType = eventData.EventType,
                            Id = eventData.Id,
                            Result = jsonDoc.RootElement
                        });
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }
    }
}