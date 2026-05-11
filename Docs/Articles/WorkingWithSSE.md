# Working with Server-Sent Events (SSE) in Casper .NET SDK

Casper nodes emit a continuous stream of events over HTTP using the [Server-Sent Events (SSE)](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events) protocol. The Casper .NET SDK provides the `ServerEventsClient` class to connect to this stream and react to network activity in real time.

## Creating a ServerEventsClient

Instantiate `ServerEventsClient` with the host and port of a node's event stream endpoint. By default, the client expects a **Casper 2.x** node. To connect to a **Casper 1.x** node, set `nodeVersion` to `1`.

```csharp
using Casper.Network.SDK.SSE;

// Connect to a Casper 2.x node (default)
var sse = new ServerEventsClient("127.0.0.1", 18101);

// Connect to a Casper 1.x node
var sseV1 = new ServerEventsClient("127.0.0.1", 18101, nodeVersion: 1);
```

The `nodeVersion` property controls how channels are mapped:
- **Casper 1.x** uses three separate channels: `main`, `deploys`, and `sigs`.
- **Casper 2.x** uses a single consolidated channel.

## Adding event callbacks

Subscribe to events by registering one or more callback methods. Each callback is associated with an `EventType` and a unique name.

```csharp
sse.AddEventCallback(EventType.BlockAdded, "block-cb", (SSEvent evt) =>
{
    var blockAdded = evt.Parse<BlockAdded>();
    Console.WriteLine($"New block: #{blockAdded.Block.Height} — {blockAdded.Block.Hash}");
});
```

You can also subscribe to multiple event types at once using bitwise flags, or use `EventType.All` to receive every event:

```csharp
sse.AddEventCallback(
    EventType.DeployAccepted | EventType.DeployProcessed,
    "deploy-cb",
    (SSEvent evt) =>
    {
        Console.WriteLine($"Deploy event: {evt.EventType}");
    });
```

To start listening, call `StartListening()`. This blocks until you call `StopListening()` or the connection is lost.

```csharp
sse.StartListening();
Console.WriteLine("Press Enter to stop listening...");
Console.ReadLine();
await sse.StopListening();
```

## Event types

The following table lists all events that a Casper node can emit. Some events are specific to Casper 2.x nodes.

| Event | Description | Casper Version |
|---|---|---|
| `ApiVersion` | Sent when a client first connects. Contains the node's API version. | All |
| `BlockAdded` | A new block has been produced by the network. | All |
| `DeployAccepted` | A legacy `Deploy` has been accepted by the node. | All |
| `DeployProcessed` | A legacy `Deploy` has been executed. | All |
| `DeployExpired` | A legacy `Deploy` has expired without being processed. | All |
| `TransactionAccepted` | A `TransactionV1` (or legacy `Deploy`) has been accepted. | 2.x |
| `TransactionProcessed` | A `TransactionV1` (or legacy `Deploy`) has been executed. | 2.x |
| `TransactionExpired` | A `TransactionV1` (or legacy `Deploy`) has expired. | 2.x |
| `FinalitySignature` | A validator has signed a block, indicating finality. | All |
| `Fault` | A validator fault has been detected in the current era. | All |
| `Step` | A new step (reward distribution) has occurred. | All |

## Parsing events

Each `SSEvent` contains a `Result` property with the raw JSON payload. Use the `Parse<T>()` method to deserialize it into a strongly-typed object.

### BlockAdded

```csharp
sse.AddEventCallback(EventType.BlockAdded, "blocks", (SSEvent evt) =>
{
    var blockAdded = evt.Parse<BlockAdded>();
    var block = blockAdded.Block;

    Console.WriteLine($"Height: {block.Height}");
    Console.WriteLine($"Era: {block.EraId}");
    Console.WriteLine($"Transactions: {block.Transactions?.Count ?? 0}");
});
```

### DeployAccepted and DeployProcessed

```csharp
sse.AddEventCallback(EventType.DeployAccepted, "deploy-accepted", (SSEvent evt) =>
{
    var deploy = evt.Parse<DeployAccepted>();
    Console.WriteLine($"Deploy accepted: {deploy.Hash}");
});

sse.AddEventCallback(EventType.DeployProcessed, "deploy-processed", (SSEvent evt) =>
{
    var deploy = evt.Parse<DeployProcessed>();
    Console.WriteLine($"Deploy processed: {deploy.DeployHash}");
    Console.WriteLine($"Block hash: {deploy.BlockHash}");
});
```

### TransactionAccepted and TransactionProcessed (Casper 2.x)

On Casper 2.0 nodes, `TransactionAccepted` contains the full transaction object (either a `Deploy` or a `TransactionV1`). `TransactionProcessed` and `TransactionExpired` contain only the transaction hash.

```csharp
sse.AddEventCallback(EventType.TransactionAccepted, "tx-accepted", (SSEvent evt) =>
{
    var tx = evt.Parse<Transaction>();
    Console.WriteLine($"Transaction accepted: {tx.Hash}");
});

sse.AddEventCallback(EventType.TransactionProcessed, "tx-processed", (SSEvent evt) =>
{
    var txProcessed = evt.Parse<TransactionProcessed>();
    Console.WriteLine($"Transaction processed: {txProcessed.TransactionHash}");
    Console.WriteLine($"Block hash: {txProcessed.BlockHash}");
});
```

### FinalitySignature

`FinalitySignature` is a versioned type. On Casper 2.x nodes, it includes `BlockHeight` and `ChainNameHash`. You can cast it to the appropriate version if needed.

```csharp
sse.AddEventCallback(EventType.FinalitySignature, "signatures", (SSEvent evt) =>
{
    var sig = evt.Parse<FinalitySignature>();
    Console.WriteLine($"Block {sig.BlockHash} finalized by {sig.Signature}");

    if (sig.Version == 2)
    {
        var sigV2 = (FinalitySignatureV2)sig;
        Console.WriteLine($"Block height: {sigV2.BlockHeight}");
    }
});
```

### Fault

```csharp
sse.AddEventCallback(EventType.Fault, "faults", (SSEvent evt) =>
{
    var fault = evt.Parse<Fault>();
    Console.WriteLine($"Validator fault in era {fault.EraId}");
});
```

### Step

```csharp
sse.AddEventCallback(EventType.Step, "steps", (SSEvent evt) =>
{
    var step = evt.Parse<Step>();
    Console.WriteLine($"Step event with {step.Effect.Count} transforms");
});
```

## Resuming from a specific event ID

You can request events starting from a specific ID. This is useful for resuming after a disconnect or for catching up on missed events.

```csharp
sse.AddEventCallback(EventType.BlockAdded, "blocks", (SSEvent evt) =>
{
    // ...
}, startFrom: 12345);
```

## Error handling and reconnection

The `ServerEventsClient` automatically retries every 5 seconds if the connection drops. You can implement additional error handling inside your callbacks:

```csharp
sse.AddEventCallback(EventType.All, "catch-all", (SSEvent evt) =>
{
    try
    {
        // parse and handle event
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to handle {evt.EventType}: {ex.Message}");
    }
});
```

## Complete example

The following example listens to all events and prints details for the most common ones:

```csharp
using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.SSE;
using Casper.Network.SDK.Types;

public class SseExample
{
    public static async Task Main(string[] args)
    {
        var sse = new ServerEventsClient("127.0.0.1", 18101);

        sse.AddEventCallback(EventType.All, "all-events", (SSEvent evt) =>
        {
            try
            {
                switch (evt.EventType)
                {
                    case EventType.ApiVersion:
                        Console.WriteLine($"API Version: {evt.Result.GetRawText()}");
                        break;

                    case EventType.BlockAdded:
                        var block = evt.Parse<BlockAdded>().Block;
                        Console.WriteLine($"Block #{block.Height} — {block.Hash}");
                        break;

                    case EventType.TransactionAccepted:
                        var tx = evt.Parse<Transaction>();
                        Console.WriteLine($"Tx accepted: {tx.Hash}");
                        break;

                    case EventType.TransactionProcessed:
                        var txProc = evt.Parse<TransactionProcessed>();
                        Console.WriteLine($"Tx processed: {txProc.TransactionHash}");
                        break;

                    case EventType.DeployAccepted:
                        var deploy = evt.Parse<DeployAccepted>();
                        Console.WriteLine($"Deploy accepted: {deploy.Hash}");
                        break;

                    case EventType.DeployProcessed:
                        var deployProc = evt.Parse<DeployProcessed>();
                        Console.WriteLine($"Deploy processed: {deployProc.DeployHash}");
                        break;

                    case EventType.FinalitySignature:
                        var sig = evt.Parse<FinalitySignature>();
                        Console.WriteLine($"Finality for block {sig.BlockHash}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling {evt.EventType}: {ex.Message}");
            }
        });

        sse.StartListening();

        Console.WriteLine("Listening. Press Enter to stop.");
        Console.ReadLine();

        await sse.StopListening();
    }
}
```
