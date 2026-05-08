# Working with TransactionV1

Casper 2.0 introduced a new transaction model called `TransactionV1`. It replaces the legacy `Deploy` model for many operations while remaining backward compatible: nodes still accept legacy deploys, but new features (such as the fixed pricing mode and native auction operations) require the new transaction type.

The Casper .NET SDK provides a fluent `TransactionBuilder` API to create `TransactionV1` objects without manually assembling low-level fields.

> **Note:** If you are migrating from SDK v2.x, read the [Migration Guide](./Casper20MigrationGuide.md) for a side-by-side comparison of `Deploy` and `TransactionV1`.

## Transaction structure

A `TransactionV1` consists of:

- **Hash** — A Blake2b hash of the payload.
- **Payload** — Contains the initiator address, chain name, timestamp, TTL, pricing mode, runtime arguments, invocation target, entry point, and scheduling.
- **Approvals** — One or more signatures from the initiator account.

## Pricing modes

Casper 2.0 supports three pricing modes. You must choose one when building a transaction.

| Mode | Description | Typical usage |
|---|---|---|
| `PaymentLimited` | The initiator specifies a maximum payment amount and a gas price tolerance. | Legacy-style payments with a cap. |
| `Fixed` | The cost is determined by the network's cost table based on the transaction category (lane). | Casper 2.0 no-fee model. |
| `Prepaid` | Reserved for future use. Not currently supported by the network. | — |

> **Note:** `PaymentLimited` is the only pricing mode currently activated on Casper testnet and mainnet. `Fixed` and `Prepaid` are not yet enabled on live networks.

The `TransactionBuilder` provides a convenient `.Payment(ulong amount, byte gasPriceTolerance = 1)` method that automatically selects `PaymentLimited` mode. This is the recommended way to set payment for transactions targeting live networks:

```csharp
// PaymentLimited: pay up to 2.5 billion motes, tolerate gas price up to 1
.Payment(2_500_000_000, 1)
```

## Building a native transfer

The simplest transaction is a native CSPR transfer between accounts. Use `Transaction.NativeTransferBuilder`:

```csharp
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;

public class TransferExample
{
    public static async Task Main(string[] args)
    {
        var client = new NetCasperClient("http://127.0.0.1:11101/rpc");
        var chainName = "casper-net-1";

        var sourceKey = KeyPair.FromPem("source_sk.pem");
        var targetPK = PublicKey.FromPem("target_pk.pem");

        var transaction = new Transaction.NativeTransferBuilder()
            .From(sourceKey.PublicKey)
            .Target(targetPK)
            .Amount(25_000_000_000) // 25 CSPR in motes
            .Id(DateUtils.ToEpochTime(DateTime.Now))
            .ChainName(chainName)
            .Payment(100_000_000, 1)
            .Build();

        transaction.Sign(sourceKey);

        var response = await client.PutTransaction(transaction);
        var txHash = response.GetTransactionHash();

        Console.WriteLine($"Transaction sent: {txHash}");

        // Wait for execution (up to 2 minutes)
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        var result = await client.GetTransaction(txHash, tokenSource.Token);

        var execResult = result.Parse().ExecutionInfo.ExecutionResult;
        Console.WriteLine($"Cost: {execResult.Cost}");
    }
}
```

### Optional transfer parameters

- **Source** — By default, the transfer debits the initiator's main purse. You can specify a different source purse with `.Source(uref)`.
- **Target** — The recipient can be a `PublicKey`, `AccountHashKey`, `URef`, or `AddressableEntityKey`.
- **Id** — An optional transfer ID (useful for idempotency).

```csharp
var transaction = new Transaction.NativeTransferBuilder()
    .From(sourceKey.PublicKey)
    .Source(new URef("uref-...-007"))
    .Target(new AccountHashKey(targetPK))
    .Amount(1_000_000_000)
    .Id(42)
    .ChainName(chainName)
    .Payment(100_000_000, 1)
    .Build();
```

## Building auction transactions

The SDK provides dedicated builders for native auction operations. These do not require a contract hash because they target the network's built-in auction contract.

### AddBid

```csharp
var transaction = new Transaction.NativeAddBidBuilder()
    .From(validatorKey.PublicKey)
    .Validator(validatorKey.PublicKey)
    .Amount(1_000_000_000_000) // 1,000 CSPR
    .DelegationRate(10)
    .MinimumDelegationAmount(500_000_000_000)
    .MaximumDelegationAmount(2_000_000_000_000)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

### Delegate

```csharp
var transaction = new Transaction.NativeDelegateBuilder()
    .From(delegatorKey.PublicKey)
    .Validator(validatorKey.PublicKey)
    .Amount(500_000_000_000)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

### Undelegate

```csharp
var transaction = new Transaction.NativeUndelegateBuilder()
    .From(delegatorKey.PublicKey)
    .Validator(validatorKey.PublicKey)
    .Amount(100_000_000_000)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

### WithdrawBid

```csharp
var transaction = new Transaction.NativeWithdrawBidBuilder()
    .From(validatorKey.PublicKey)
    .Validator(validatorKey.PublicKey)
    .Amount(500_000_000_000)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

### Redelegate

```csharp
var transaction = new Transaction.NativeRedelegateBuilder()
    .From(delegatorKey.PublicKey)
    .Validator(oldValidatorKey.PublicKey)
    .NewValidator(newValidatorKey.PublicKey)
    .Amount(250_000_000_000)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

### ActivateBid

```csharp
var transaction = new Transaction.NativeActivateBidBuilder()
    .From(validatorKey.PublicKey)
    .Validator(validatorKey.PublicKey)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

## Calling a stored contract

To call an entry point in a stored contract, use `Transaction.ContractCallBuilder`:

```csharp
var runtimeArgs = new List<NamedArg>
{
    new NamedArg("recipient", CLValue.Key(new AccountHashKey(recipientPK))),
    new NamedArg("amount", CLValue.U256(100))
};

var transaction = new Transaction.ContractCallBuilder()
    .From(senderKey.PublicKey)
    .ByHash("hash-deadbeef...")
    .EntryPoint("transfer")
    .RuntimeArgs(runtimeArgs)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

You can also reference contracts by name, package hash, or package name:

```csharp
// By contract name (stored in the caller's named keys)
.ByName("my_contract")

// By package hash, optionally specifying a version
.ByPackageHash("hash-...", version: 1)

// By package name
.ByPackageName("my_package", version: 1)
```

## Deploying session code (WASM)

To deploy new session code, use `Transaction.SessionBuilder`:

```csharp
var wasmBytes = await File.ReadAllBytesAsync("contract.wasm");

var runtimeArgs = new List<NamedArg>
{
    new NamedArg("name", "MyToken"),
    new NamedArg("symbol", "MTK")
};

var transaction = new Transaction.SessionBuilder()
    .From(deployerKey.PublicKey)
    .Wasm(wasmBytes)
    .RuntimeArgs(runtimeArgs)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

For install/upgrade operations, call `.InstallOrUpgrade()`:

```csharp
var transaction = new Transaction.SessionBuilder()
    .From(deployerKey.PublicKey)
    .Wasm(wasmBytes)
    .InstallOrUpgrade()
    .RuntimeArgs(runtimeArgs)
    .ChainName(chainName)
    .Payment(2_500_000_000, 1)
    .Build();
```

## Signing a transaction

After building, sign the transaction with the initiator's private key:

```csharp
transaction.Sign(keyPair);
```

You can also add additional approvals (multi-sig) using `AddApproval()`.

## Sending and monitoring a transaction

Use `PutTransaction()` to send the transaction to the network, then poll `GetTransaction()` to wait for execution:

```csharp
var putResponse = await client.PutTransaction(transaction);
var txHash = putResponse.GetTransactionHash();

var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
var getResponse = await client.GetTransaction(txHash, tokenSource.Token);

var execInfo = getResponse.Parse().ExecutionInfo;
Console.WriteLine($"Block hash: {execInfo.BlockHash}");
Console.WriteLine($"Cost: {execInfo.ExecutionResult.Cost}");
Console.WriteLine($"Consumed gas: {execInfo.ExecutionResult.Consumed}");
```

## Validating and verifying a transaction

Before sending, you can validate the transaction hash and verify signatures:

```csharp
if (!transaction.ValidateHashes(out string hashError))
{
    Console.WriteLine($"Hash validation failed: {hashError}");
}

if (!transaction.VerifySignatures(out string sigError))
{
    Console.WriteLine($"Signature verification failed: {sigError}");
}
```

## Serialization

You can serialize a transaction to JSON and save it to a file for later use:

```csharp
var json = transaction.SerializeToJson();
File.WriteAllText("transaction.json", json);

// Load it back later
var loaded = TransactionV1.Load("transaction.json");
```
