# Querying Balances

On Casper 2.0 networks, the recommended way to query an account's balance is the `QueryBalance()` method. It replaces the older `GetAccountBalance()` method (removed in SDK v3) and supports multiple ways to identify a purse or account.

## Balance identifiers

`QueryBalance()` accepts any object implementing the `IPurseIdentifier` interface. The following types implement this interface:

| Type | Description |
|---|---|
| `PublicKey` | The public key of an account. |
| `AccountHashKey` | The account hash derived from a public key. |
| `URef` | The main purse or any other purse URef. |
| `AddressableEntityKey` | An entity key (account or smart contract) introduced in Casper 2.0. |

## Querying by public key

The simplest way to get a balance is to pass the account's public key:

```csharp
using System;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;

public class BalanceExample
{
    public static async Task Main(string[] args)
    {
        var client = new NetCasperClient("http://127.0.0.1:11101/rpc");

        var publicKey = PublicKey.FromHexString(
            "0184f6d260F4EE6869DDB36affe15456dE6aE045278FA2f467bb677561cE0daD55");

        try
        {
            var response = await client.QueryBalance(publicKey);
            var balance = response.Parse().BalanceValue;
            Console.WriteLine($"Balance: {balance} motes");
        }
        catch (RpcClientException e)
        {
            Console.WriteLine($"RPC Error: {e.RpcError.Message}");
        }
    }
}
```

## Querying by purse URef

If you already know the main purse URef of an account, you can query it directly:

```csharp
var mainPurse = new URef("uref-0d0b57865e41b9e39170c038993997af432f66545f56838f1bf602c6d56e0e54-007");
var response = await client.QueryBalance(mainPurse);
Console.WriteLine($"Purse balance: {response.Parse().BalanceValue} motes");
```

## Querying by account hash

You can also use the account hash:

```csharp
var accountHash = new AccountHashKey("account-hash-56befc13a6fd62e18f361700a5e08f966901c34df8041b36ec97d54d605c23de");
var response = await client.QueryBalance(accountHash);
Console.WriteLine($"Balance: {response.Parse().BalanceValue} motes");
```

## Querying by AddressableEntity (Casper 2.0)

On Casper 2.0 networks, accounts and smart contracts are represented as `AddressableEntity`. You can query the balance of an entity using its key:

```csharp
var entityKey = new AddressableEntityKey(
    "entity-account-56befc13a6fd62e18f361700a5e08f966901c34df8041b36ec97d54d605c23de");

var response = await client.QueryBalance(entityKey);
Console.WriteLine($"Entity balance: {response.Parse().BalanceValue} motes");
```

## Querying at a specific block

By default, `QueryBalance()` returns the balance at the latest block. You can query at a specific block hash or height:

```csharp
// By block hash
var response = await client.QueryBalance(publicKey, "block-hash-...");

// By block height
var response = await client.QueryBalance(publicKey, 12345UL);
```

## Querying balance details

In addition to the total balance, you can retrieve detailed balance information (available and total hold amounts) using `QueryBalanceDetails()`:

```csharp
var response = await client.QueryBalanceDetails(publicKey);
var details = response.Parse();

Console.WriteLine($"Total balance: {details.TotalBalance}");
Console.WriteLine($"Available balance: {details.AvailableBalance}");
Console.WriteLine($"Total holds: {details.TotalHolds}");
```

## Legacy method: GetBalance

If you need to use the older `state_get_balance` RPC method directly (for example, with a specific state root hash), use `GetBalance()`:

```csharp
var response = await client.GetBalance("uref-...-007", stateRootHash: "...");
Console.WriteLine($"Balance: {response.Parse().BalanceValue}");
```
