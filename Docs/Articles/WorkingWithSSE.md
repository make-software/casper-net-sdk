# Working with Server-Sent Events (SSE) in Casper .NET SDK 3.x

The new Casper Node **2.x** version introduced changes to the SSE protocol. To support these changes, the Casper .NET SDK has been updated to version **3.x**, introducing a new optional parameter `nodeVersion` in the `ServerEventsClient`.

## Creating `ServerEventsClient` for Node v2.x (Default)

When working with the new Casper Node version **2.x**, instantiate the `ServerEventsClient` without specifying the `nodeVersion`. The default value is set to `2`:

```csharp
var sse = new ServerEventsClient(eventIpAddress, localNetPort, nodeVersion: 1); // For versions 1.x

var sse = new ServerEventsClient(eventIpAddress, localNetPort); // The default value is 2
```