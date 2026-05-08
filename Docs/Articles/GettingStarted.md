# Getting started

Get started with `Casper.Network.SDK` quickly by adding a reference to the NuGet package or linking to the source code project.

## Prerequisites

The SDK targets **.NET 8**. Make sure you have the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your machine.

## Add the NuGet package to your project

The `Casper.Network.SDK` is published as a NuGet package on [nuget.org](https://www.nuget.org/packages/Casper.Network.SDK).

To add a reference to the SDK in your project, use the Package Manager in Visual Studio or the `dotnet` CLI tool.

### Package Manager (Windows)
```
Install-Package Casper.Network.SDK
``` 

### dotnet CLI tool (Windows/Mac/Linux)
```
dotnet add package Casper.Network.SDK
``` 

## Add a reference to the source code

Clone the GitHub repository in your solution folder and add a reference in your project to the `Casper.Network.SDK.csproj` file.

```bash
git clone https://github.com/make-software/casper-net-sdk.git
cd your-project
dotnet add reference ../casper-net-sdk/Casper.Network.SDK/Casper.Network.SDK.csproj
```

## Create a minimal console application

Create a new console app and add the SDK package:

```bash
dotnet new console -o GetStarted
cd GetStarted
dotnet add package Casper.Network.SDK
```

Replace the contents of `Program.cs` with the following example. This program connects to a public Casper testnet node and prints the latest block height:

```csharp
using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;

namespace GetStarted
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new NetCasperClient("https://rpc.testnet.casperlabs.io/rpc");

            try
            {
                var response = await client.GetBlock();
                var block = response.Parse().Block;
                Console.WriteLine($"Latest block height: {block.Height}");
                Console.WriteLine($"Block hash: {block.Hash}");
            }
            catch (RpcClientException e)
            {
                Console.WriteLine($"RPC Error: {e.RpcError.Message}");
            }
        }
    }
}
```

Run the program:

```bash
dotnet run
```

## Next steps

- Read [Working with TransactionV1](./WorkingWithTransactionV1.md) to learn how to send the new Casper 2.0 transaction type.
- Read [Key management](./KeyManagement.md) to learn how to create and load key pairs for signing transactions.
- Browse the [Examples](../Examples/index.md) for more complete sample programs.
- Follow one of the [Tutorials](../Tutorials/index.md) to deploy and interact with smart contracts.
