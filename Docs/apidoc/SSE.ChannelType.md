---
uid: Casper.Network.SDK.SSE.ChannelType
summary: An enumeration with the event stream channels offered by a Casper node.
remarks: *content
---
A Casper node that permits a client to subscribe to the event stream offers three different channels:
`Main`, `Deploys` and `Sigs`.

The client does not subscribe to an specific channel. Instead, it must add event callback methods that
subscribe to the event types needed and then the channels are automatically joined.
