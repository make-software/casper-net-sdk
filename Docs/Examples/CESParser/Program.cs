using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.CES;
using Casper.Network.SDK.SSE;

namespace Casper.NET.SDK.Examples
{
    public static class CESParserExample
    {
        private static string nodeAddress = "http://127.0.0.1:11101/rpc";
        private static string sseHost = "127.0.0.1";
        private static int ssePort = 18101;

        private static NetCasperClient casperSdk;
        private static IReadOnlyList<CESContractSchema> watchedSchemas;

        public static void ListenEvents(int startFrom)
        {
            var sse = new ServerEventsClient(sseHost, ssePort);

            sse.AddEventCallback(
                EventType.TransactionProcessed,
                "ces-parser-cb",
                (SSEvent evt) =>
                {
                    try
                    {
                        var transaction = evt.Parse<TransactionProcessed>();
                        var events = CESParser.GetEvents(
                            transaction.ExecutionResult.Effect,
                            watchedSchemas);

                        Console.WriteLine("TransactionProcessed: " + transaction.TransactionHash);

                        if (events.Count == 0)
                            return;

                        foreach (var parsedEvent in events)
                        {
                            Console.WriteLine("CES event:");
                            Console.WriteLine(PrettyPrintEvent(parsedEvent));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                },
                startFrom: startFrom);

            sse.StartListening();

            Console.WriteLine("Listening to TransactionProcessed events.");
            Console.WriteLine("Press Enter to stop listening.");
            Console.ReadLine();
            Console.WriteLine("Terminating...");

            sse.StopListening().Wait();

            Console.WriteLine("Terminated");
        }

        public static async Task Main(string[] args)
        {
            var contractHash = args.Length > 0
                ? args[0]
                : "hash-1262d06e53125ea098187fb4d1d5b10a7afed48e5e5eef182ed992fc5b100349";

            try
            {
                casperSdk = new NetCasperClient(nodeAddress);

                Console.WriteLine("Loading CES schema for contract: " + contractHash);
                var schema = await CESContractSchema.LoadAsync(casperSdk, contractHash);
                watchedSchemas = new List<CESContractSchema> { schema };

                Console.WriteLine("Resolved contract hash : " + schema.ContractHash);
                Console.WriteLine("Schema events URef     : " + schema.EventsURef);
                Console.WriteLine("Event types:");
                foreach (var eventSchema in schema.Events)
                {
                    Console.WriteLine("- " + eventSchema.Key);
                }

                ListenEvents(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static string PrettyPrintEvent(CESEvent evt)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };

            return JsonSerializer.Serialize(evt, options);
        }
    }
}