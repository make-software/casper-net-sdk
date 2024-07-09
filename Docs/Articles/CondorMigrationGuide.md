# Migration from Casper .NET SDK v2.x to v3.0

To migrate your application from Casper .NET SDK v2.x to Casper .NET SDK v3.0 you'll need to make several
changes on your code. The good news is that with version 3 of the SDK your application will be compatible with
Casper v1.5.6 as well as with Casper v2.0 (aka Condor).

This guide does not replace other Condor-related documents that introduce the changes in the new version of the Casper
network
software. We encourage the reader to get familiar with the new concepts first.

## Blocks

With Condor, block records use a new format to accommodate more types of Transactions and surface previously embedded
information. New blocks will use the new format; historical blocks retain their original format.

In the SDK, this mean you can deal with two versions of blocks. `BlockV1` are blocks produced before Condor, i.e. within
Casper 1.x protocol version. `BlockV2` are blocks produced after Condor upgrade.

To facilitate handling different versions, the SDK implements the type `Block` which can be either a V1 or V2 type in
the network. This type contains all the data you may want to query:

```csharp
public class Block
{
        public int Version { get; init; }
        public string Hash { get; init; }
        public string AccumulatedSeed { get; init; }
        public ulong EraId { get; init; }
        public ulong Height { get; init; }
        public string ParentHash { get; init; }
        public string ProtocolVersion { get; init; }
        public bool RandomBit { get; init; }
        public string StateRootHash { get; init; }
        public string Timestamp { get; init; }
        public EraEnd EraEnd { get; init; }
        public UInt16 CurrentGasPrice { get; init; }
        public Proposer Proposer { get; init; }
        public string LastSwitchBlockHash { get; init; }
        public List<BlockTransaction> Transactions { get; init; }
        public List<List<UInt16>> RewardedSignatures { get; init; }
}
```

Note that `Block` does not have a header nor body parts.

Also, be aware that some properties may be `null` if they're not part of the versioned block. For
example, `LastSwitchBlockHas` is present only for V2 blocks.

### Recovering the versioned block object

If, for any reason, you need to work with the original format of the block, you can cast a `Block` to a `BlockV1`
or `BlockV2`. For example:

```csharp
if (block.Version == 2) {
    var blockv2 = (BlockV2)block;
    // ...
} else if (block.Version == 1) {
    var blockv1 = (BlockV1)block;
    // ...
}    
```

### EraEnd

`EraEnd` structure also differs for switch blocks produced before and after Condor. In most cases you can just use
the `EraEnd` object from the common `Block` class. But again, if you need it, you can recover an `EraEndV1` object from
a V1 block:

```csharp
if (block.Version == 1) {
    var blockv1 = (BlockV1)block;
    var eraEnd = blockv1.Header.EraEnd;
    // ...
}    
```

### Transactions included in a block

Blocks produced on Casper 1.x contain a list of deploys which differentiate between native transfers and all other types
of deploys. On Condor, transactions (this includes legacy deploys too) have a category field which determines the
processing lane the transaction is sent to. The chainspec of the network determines the properties of each lane:

1. Maximum transaction size in bytes for a given transaction in a certain lane.
2. Maximum args length size in bytes for a given transaction in a certain lane.
3. Transaction gas limit size in motes for a given transaction in a certain lane.
4. The maximum number of transactions the lane can contain for one block.

The categories (i.e. lanes) defined so far are:

1. Mint (native CSPR transfers) transactions
2. Auction (native interaction with the Auction contract) transactions
3. Install/Upgrade transactions
4. Large transactions
5. Medium transactions
6. Small transactions

A `Block` contains a list of transactions:

```csharp
public List<BlockTransaction> Transactions { get; init; }
```

For each transaction, the category, the Version (either a legacy `Deploy` or the new `TransactionV1`) and its `Hash` is
provided.

`Deploy`s in V1 blocks are categorized as `Mint` for native transfer deploys and `Large` for all the rest.

In this list, each transaction has

### Block height type

The type to represent block height is now `ulong` everywhere. `int` was used in some methods or types in the previous
version. That's not the case with v3.

## Account/Contract Merge

On Condor, accounts and contracts are stored with the new type `AddressableEntity`. The `EntityKind` property in
this type permits to know whether the record is an `Account`, a stored `SmartContract` or a `System` contract.

### GetEntity RPC method

Use the new method`GetEntity(IEntityIdentifier)` in the RPC interface to retrieve a
record. `PublicKey`, `AccountHashKey`
and `AddressableEntityKey` implement the `IEntityIdentiifer` interface and can be used to retrieve
and `AddressableEntity` from the network. While `PublicKey` and `AccountHashKey`are known, the `AddressableEntitykey` is
new but the developer must come familiar with it to work with Condor. Some examples of this key are:

```
entity-account-2f3fb80d362ad0a922f446915a259c9aaec9ba99292b3e50ff2359c458007309
entity-contract-a5cf5917505ef60a6f0df395dd19e86a0f075d00f2e6ce49f5aa0e18f6e26f5d
entity-system-a1b5f200a58533875ef83cb98de14f128342b34162cbc14d4f41f3ccbc451dc3
```

### Legacy accounts

Existing accounts are migrated either during the Condor upgrade or when they interact with the network for
the first time after the upgrade. The exact time depends on the `chainspec` agreed for the Condor upgrade. Hash values
for the `AccountHashKey` and the `EntityKey` are shared and remain unchanged for migrated accounts.

Non-migrated accounts can be retrieved also with the new `GetEntity` method. In this case, the response contains the
account info in the `LegacyAccount` property instead of in the `Entity` property.

Alternatively, the legacy method `GetAccountInfo` works also for non-migrated accounts.

### Contract information

Similar to accounts, contract records are migrated also to the new `AddressableEntity`. After the migration,
only `GetEntity` method can be used to retrieve contract information. Hash values for contracts and packages remain
unchaged for the migrated records.

To retrieve information about the contract package use the `QueryGlobalState` method with the `PackageKey` of the
contract.

### Backwards compatibility

When using the SDK v3 with a Casper v1.x network, only `GetAccountInfo` can be used to retrieve account information. For
contract and package information use `QueryGlobalState`.

## Balances

The new `NoFee` mode in Condor introduces the concept of a 'balance hold'. In this mode of operation, for each
transaction
the network holds the amount paid for its execution in the paying purse. Thus, entities have a total balance and an
available balance, where available balance is equal to the total balance minus the balance holds.

To get a detailed information of an entity balance use the new method `QueryBalanceDetails(IPurseIdentifier)` with a
purse identifier. `PublicKey`, `AccountHashKey`, `AddressableEntityKey` and `URef` keys implement the `IPurseIdentifier`
interface and
can be used to retrieve the balance details for an entity.

`GetAccountBalance` has been renamed to `GetBalance` since this method can be used for any type of entity, not only for
accounts.

## Deploys and Transactions

Condor introduces a new transaction model to support advanced use cases. While `Deploy`s continue to work in Condor,
this type is deprecated and Casper recommends to switch to the new `TransactionV1` type.

Similar to the `DeployTemplates` class which provided deploy templates for most common use cases, we plan to implement
a `TransactionV1Templates` class for the new model of transactions in a next release of this SDK.

Use the new method `PutTransaction` to send a `TransactionV1` to the network. To retrieve an accepted transaction use
the new `GetTransaction` method. `GetTransaction` can be used also to retrieve a `Deploy`. For non processed
transactions the `ExecutionInfo` in the response is null. Upon processing, this property contains all information about
the execution, including cost, payments, errors (if any) and execution effects.

### Payments and costs

For a transaction (old and new types) processed in Condor, the execution results object contain three properties related
to the gas consumed and the CSPR tokens paid:

- `limit`: The maximum allowed gas limit for the transaction.
- `consumed`: How much gas was consumed executing the transaction.
- `cost`: How much CSPR was paid/held for the transaction.

In the `NoFee` model, the user does not specify any amount for payment. Instead, he must use the `Fixed` pricing mode
with a gas price tolerance. The network chainspec defines the gas limit for each of the transaction categories (see
'Transactions included in a block' section above). Then for each era the network determines a gas price based on the
previous era load. This price works as a multiplier for the consumed gas and relates to the gas price
tolerance specified in the transaction. If the tolerance is lower than the gas price, the transaction won't be
processed. The consumed gas in a transaction must be always lower than the limit or it will fail with an out of gas
error. Finally, the cost in the no-fee model is the limit multiplied by the gas price.

### Execution results

The `ExecutionResult` class is a versioned object. Deploys processed before Condor upgrade are stored in the global
state as `ExecutionResultV1`records. And deploys and transactions processed after Condor upgrade are stored
as `ExecutionResultV2` records.

The `GetTransaction` method always returns a `ExecutionResult` instance regardless the version. It has the same fields
than `ExecutionResultV2`. The user can obtain the original structure with casting this instance to the correct version:

```csharp
if (executionResult.Version == 2) {
    var executionResultV2 = (ExecutionResultV2)executionResult;
    // ...
} else if (executionResult.Version == 1) {
    var executionResultV1 = (ExecutionResultV1)executionResult;
    // ...
}   
```

The `Effect` property contains a list of `Transforms` that modify the global state. Note that the `ExecutionEffect`
which contained also a list of operations in addition to the transforms has been removed in Condor execution results.

## Auction contract

The auction contract also has changed in Condor. If your application tracks validator bids, rewards and delegators,
you'll need to rework the way network responses are parsed and interpreted. A complete description of the changes cannot
covered in this guide.

## Server Sent Events

The `ServerEventsClient` class can listen to both an event stream from a `v1.x` node as well as from `v2.x`.

### Blocks

`BlockedAdded` event contains a `Block` object. When needed, the original block structure `BlockV1` or `BlockV2` can be
obtained with cast operator as described above.

### Transactions

On Condor, the events `DeployAccepted`, `DeployProcessed` and `DeployExpired` are replaced with the
equivalent `TransactionAccepted`, `TransactionProcessed` and `TransactionExpired`. These new events are emitted for
both `Deploy` and `TransactionV1` types of transactions.

A `TransactionAccepted` event contains a `Transaction` property with either a `Deploy` or a `TransactionV1`.

`TransactionProcessed` and `TransactionExpired` events contain a `TransactionHash` property with either a deploy hash or
a transaction version 1 hash.

### Finality signatures

The `FinalitySignature` event contains an instance of the versioned `FinalitySignature` class. Version 2 of this type is
an extension that contains all properties in version 1 plus the block height and the chain name hash.

The user can obtain the original structure with casting this instance to the correct version:

```csharp
if (finalitySignature.Version == 2) {
    var finalitySignatureV2 = (FinalitySignatureV2)finalitySignature;
    // ...
} else if (finalitySignature.Version == 1) {
    var finalitySignatureV1 = (FinalitySignatureV1)finalitySignature;
    // ...
}   
```

## Other changes

### Last switch block hash

For a Condor network, it is possible to get the latest switch block hash with the `GetNodeStatus` method. The response
contains the `LatestSwitchBlockHash` property with this value.

Also, for blocks produced in Casper 2.0, the `Block` instance contains the previous switch block hash in
the `LastSwitchBlockHash` property.

### Checksums (CEP-57)

On SDK v3 only public keys are checksummed. The rest of keys and hashes are not checksummed anymore.

