using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.SSE;

namespace Casper.NET.SDK.Examples
{
    public static class AwaitEvents
    {
        // Testnet node and port
        private static string localNetHost = "127.0.0.1";
        private static int localNetPort = 18101;
        
        static NetCasperClient casperSdk;

        // Listen to deploys channel
        //
        public static void ListenEvents(int startFrom)
        {
            // instantiate sse client with ip address and port for casper node
            //
            var sse = new ServerEventsClient(localNetHost, localNetPort);

            // add a callback to process deploy accepted events. 
            //
            sse.AddEventCallback(EventType.All, "catch-all-cb",
                (SSEvent evt) =>
                {
                    Console.WriteLine(evt.EventType);

                    try
                    {
                        if (evt.EventType == EventType.FinalitySignature)
                        {
                            var sig = evt.Parse<FinalitySignature>();
                            Console.WriteLine("Validator PK: " + sig.Signature);
                        }
                        else if (evt.EventType == EventType.BlockAdded)
                        {
                            var block = evt.Parse<BlockAdded>();
                            Console.WriteLine("Block height: " + block.Block.Height);
                        }
                        else if (evt.EventType == EventType.TransactionAccepted)
                        {
                            var transaction = evt.Parse<TransactionAccepted>();
                            Console.WriteLine("TransactionAccepted: " + transaction.Hash);
                        }
                        else if (evt.EventType == EventType.TransactionProcessed)
                        {
                            var transaction = evt.Parse<TransactionProcessed>();
                            Console.WriteLine("TransactionProcessed: " + transaction.TransactionHash.ToString());
                        }
                        else if (evt.EventType == EventType.DeployAccepted)
                        {
                            var deploy = evt.Parse<DeployAccepted>();
                            Console.WriteLine("DeployAccepted: " + deploy.Hash);
                        }
                        else if (evt.EventType == EventType.DeployProcessed)
                        {
                            var deploy = evt.Parse<DeployProcessed>();
                            Console.WriteLine("DeployProcessed: " + deploy.DeployHash);
                        }
                        else if (evt.EventType == EventType.ApiVersion)
                        {
                            Console.WriteLine(evt.Result.GetRawText());
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                },
                startFrom: startFrom);

            sse.StartListening();

            Console.WriteLine("Press Enter to stop listening.");
            Console.ReadLine();
            Console.WriteLine("Terminating...");

            sse.StopListening().Wait();

            Console.WriteLine("Terminated");
        }

        public static async Task Main(string[] args)
        {
            ListenEvents(0);
        }
    }
}
