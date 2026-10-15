# Casper Event Standard (CES) for .NET

`Casper.Network.SDK.CES` parses typed events emitted by Casper smart contracts that follow the Casper Event Standard. It supports .NET 8 and .NET Standard 2.0 and depends on `Casper.Network.SDK`.

## Installation

```bash
dotnet add package Casper.Network.SDK.CES
```

## Usage

```csharp
using System;
using Casper.Network.SDK;
using Casper.Network.SDK.CES;

var client = new NetCasperClient("http://127.0.0.1:11101/rpc");
var schema = await CESContractSchema.LoadAsync(client, contractPackageHash);

var events = CESParser.GetEvents(executionResult.Effect, new[] { schema });
foreach (var evt in events)
    Console.WriteLine(evt.Name);
```

`contractPackageHash` is the hash of the contract package to watch, and `executionResult` is the execution result of a processed transaction.

See the [CES documentation](https://make-software.github.io/casper-net-sdk/Articles/CasperEventStandard.html) and the [SSE example](https://github.com/make-software/casper-net-sdk/tree/master/Docs/Examples/CESParser) for more details.
