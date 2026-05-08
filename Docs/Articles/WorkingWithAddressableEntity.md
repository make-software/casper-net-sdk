# Working with AddressableEntity

> **Info:** `AddressableEntity` is currently not enabled on Casper testnet or mainnet.

Casper 2.0 introduced the **AddressableEntity** model, which unifies how accounts, smart contracts, and system contracts are represented on the network. Instead of treating accounts and contracts as completely separate concepts, Casper 2.0 wraps them in a common `AddressableEntity` structure.

The Casper .NET SDK provides the `GetEntity()` method to retrieve this information and the `AddressableEntityKey` type to reference entities in global state.

> **Note:** `GetEntity()` replaces `GetAccountInfo()` for Casper 2.0 networks. On a Casper 2.0 node, `GetAccountInfo()` may return legacy account data, while `GetEntity()` returns the new unified model.

## What is an AddressableEntity?

An `AddressableEntity` represents any of the following:

- **Account** — A user account with associated keys and action thresholds.
- **Smart Contract** — A Wasm-based contract deployed on the network.
- **System Contract** — A built-in contract such as Mint, HandlePayment, StandardPayment, or Auction.

Each entity has:

| Property | Description |
|---|---|
| `ProtocolVersion` | The Casper protocol version at which the entity was created or last updated. |
| `EntityKind` | Indicates whether the entity is an `Account`, `SmartContract`, or `System` contract. |
| `Package` | The package hash associated with the entity (for contracts). |
| `ByteCodeHash` | The hash of the contract's Wasm bytecode (for smart contracts). |
| `MainPurse` | The URef of the entity's main purse (used for balance queries). |
| `AssociatedKeys` | Public keys authorized to act on behalf of the entity. |
| `ActionThresholds` | Minimum weights required for deployment and key management actions. |

## Entity kinds

The `EntityKind` property tells you what type of entity you are working with:

```csharp
var entity = response.Parse().Entity;

if (entity.EntityKind.Account != null)
{
    Console.WriteLine("This is an account entity.");
}
else if (entity.EntityKind.SmartContract.HasValue)
{
    Console.WriteLine($"This is a smart contract with runtime: {entity.EntityKind.SmartContract}");
}
else if (entity.EntityKind.System.HasValue)
{
    Console.WriteLine($"This is a system contract: {entity.EntityKind.System}");
}
```

Possible `System` values are:
- `Mint`
- `HandlePayment`
- `StandardPayment`
- `Auction`

## Querying an entity by public key

The simplest way to query an entity is to pass the account's public key:

```csharp
using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

public class EntityExample
{
    public static async Task Main(string[] args)
    {
        var client = new NetCasperClient("http://127.0.0.1:11101/rpc");

        var publicKey = PublicKey.FromHexString(
            "0184f6d260F4EE6869DDB36affe15456dE6aE045278FA2f467bb677561cE0daD55");

        try
        {
            var response = await client.GetEntity(publicKey);
            var entity = response.Parse().Entity;

            Console.WriteLine($"Protocol version: {entity.ProtocolVersion}");
            Console.WriteLine($"Main purse: {entity.MainPurse}");
            Console.WriteLine($"Associated keys: {entity.AssociatedKeys.Count}");
            Console.WriteLine($"Deployment threshold: {entity.ActionThresholds.Deployment}");
        }
        catch (RpcClientException e)
        {
            Console.WriteLine($"RPC Error: {e.RpcError.Message}");
        }
    }
}
```

## Querying by account hash

You can also query using an `AccountHashKey`:

```csharp
var accountHash = new AccountHashKey("account-hash-56befc13a6fd62e18f361700a5e08f966901c34df8041b36ec97d54d605c23de");
var response = await client.GetEntity(accountHash);
var entity = response.Parse().Entity;
```

## Querying by entity address

Casper 2.0 uses a new addressing scheme for entities. You can query directly by the entity address string:

```csharp
var entityAddr = "entity-account-56befc13a6fd62e18f361700a5e08f966901c34df8041b36ec97d54d605c23de";
var response = await client.GetEntity(entityAddr);
var entity = response.Parse().Entity;
```

## Querying at a specific block

You can query the entity state at a specific block hash or height:

```csharp
// By block hash
var response = await client.GetEntity(publicKey, "block-hash-...");

// By block height
var response = await client.GetEntity(publicKey, 12345UL);
```

## Using the main purse for balance queries

One of the most common uses of `GetEntity()` is to obtain the `MainPurse` URef for balance queries:

```csharp
var response = await client.GetEntity(publicKey);
var mainPurse = response.Parse().Entity.MainPurse;

var balanceResponse = await client.QueryBalance(mainPurse);
Console.WriteLine($"Balance: {balanceResponse.Parse().BalanceValue} motes");
```

## Differences from GetAccountInfo

| Feature | `GetAccountInfo()` | `GetEntity()` |
|---|---|---|
| Casper 1.x | Returns full account info | Returns entity wrapper |
| Casper 2.x | Returns legacy account data | Returns unified entity model |
| Named keys | Included directly | May be structured differently |
| Main purse | Included | Included |
| Action thresholds | Included | Included |

For new applications targeting Casper 2.0, prefer `GetEntity()`. For backward compatibility with Casper 1.x, you can continue to use `GetAccountInfo()` or branch based on the node version.
