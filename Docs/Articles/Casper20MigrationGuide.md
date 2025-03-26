# Migration from Casper .NET SDK v2.x to v3.0

To migrate your application from Casper .NET SDK v2.x to Casper .NET SDK v3.0, you’ll need to make several changes to your code. The good news is that version 3 of this SDK keeps compatibility with Casper v1.5.6 nodes; therefore, once you update your application, it will work before and after the Casper 2.0 upgrade.

This guide outlines the changes necessary for your application. However, it's worth noting that it doesn't replace other documents introducing the new concepts in Casper 2.0 or migration guides. We strongly advise the reader to understand these new concepts first, as they will significantly aid in grasping the changes.

## Blocks

With Casper 2.0, produced blocks are stored in a new format that extends the information contained compared to old blocks. Blocks produced before the upgrade keep their original format. Thus, the SDK implements `BlockV1` and `BlockV2` classes to handle old and new block formats, respectively.

To facilitate handling different versions, the SDK also implements the type `Block`, which can represent either a V1 or V2 type in the network. This is the type obtained by default in the RPC queries and the SSE channel and contains all the data you may want to query:

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

Note that `Block` does not have a header or body parts.

Also, some properties may have a `null` value if they’re not part of the versioned block version. For example, `LastSwitchBlockHash` is present only for V2 blocks and `null` for V1 blocks.

### Recovering the versioned block object

If, for any reason, you need to work with the original format of the block, you can cast a Block into BlockV1 or BlockV2. For example:

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

EraEnd structure also differs for switch blocks produced before and after Casper 2.0. In most cases, you can just use the EraEnd object from the common Block class. But again, if necessary, you can recover an EraEndV1 object from a version 1 block instance:

```csharp
if (block.Version == 1) {
    var blockv1 = (BlockV1)block;
    var eraEnd = blockv1.Header.EraEnd;
    // ...
}    
```

### Transactions included in a block

Blocks produced on Casper v1.x contain a list of deploys that differentiate between native transfers and all other types of deploys. On Casper 2.0, transactions (this includes legacy deploys too) the payment amount determines the processing lane to which the transaction is sent. The chainspec of the network determines the properties of each lane:

1. Maximum transaction size in bytes for a given transaction in a certain lane.
2. Maximum args length size in bytes for a given transaction in a certain lane.
3. Transaction gas limit size in motes for a given transaction in a certain lane.
4. The maximum number of transactions the lane can contain for one block.

The categories (i.e., lanes) defined so far are:

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

The category, version (either a legacy Deploy or the new TransactionV1 type), and the hash are provided for each transaction.

`Deploy`s in V1 blocks are categorized as `Mint` for native transfer deploys and `Large` for all the rest. The same applies to legacy deploys sent to a Casper v2.x node.

### Block height type

The type to represent block height is now `ulong` everywhere. `int` was used in some methods or types in the previous
version. That's not the case with Casper .NET SDK v3.

## Transfers

The `Transfer` class contained in the response for a `GetBlockTransfers()` call or in a `ExecutionResult` object is also a versioned object. It contains all the information related to the transfer.

If you need to recover the original format, cast an instance to a `TransferV1` or `TransferV2` object:

```csharp
var response = await rpcClient.GetBlockTransfers();
var result = response.Parse();
foreach(var transfer in result.Transfers)
{
    if (transfer.Version == 1) {
        var transferv1 = (TransferV1)transfer;
        // ...
    }
}
```

### GetDeploy()

Response from the `GetDeploy()` method has changed. Instead of a list of `ExecutionResult` objects, it now returns an instance of `ExecutionInfo` for a processed deploy. This instance contains block information and a results object.

### Payments and costs

For a transaction (old and new versions) processed in Casper 2.0, the execution results object contains three properties related to the gas consumed and the CSPR tokens paid:

- `limit`: The maximum allowed gas limit for the transaction.
- `consumed`: Gas consumed executing the transaction.
- `cost`: CSPR paid/held for the transaction.

In the NoFee model, the user does not specify any amount for payment. Instead, he must use the Fixed pricing mode with a gas price tolerance. The network chainspec defines the gas limit for each transaction category (see the ‘Transactions included in a block’ section above). Then, the network determines a gas price for each era based on the previous era's load. This price works as a multiplier for the consumed gas and relates to the gas price tolerance specified in the transaction. The transaction won't be processed if the tolerance is lower than the gas price. The gas consumed in a transaction must always be lower than the limit, or it will fail with an out-of-gas error. Finally, the cost in the no-fee model is the limit multiplied by the gas price.

### Execution results

The `ExecutionResult` class is a versioned object. Deploys processed before the Casper 2.0 upgrade are stored in the global state as `ExecutionResultV1`records, and deploys and transactions processed after the Casper 2.0 upgrade are stored as `ExecutionResultV2` records.

For a processed transaction, the `GetTransaction` method always returns an `ExecutionResult` instance regardless of the version. It has the same fields as `ExecutionResultV2`. You can obtain the original structure by casting this instance to the correct version:

```csharp
if (executionResult.Version == 2) {
    var executionResultV2 = (ExecutionResultV2)executionResult;
    // ...
} else if (executionResult.Version == 1) {
    var executionResultV1 = (ExecutionResultV1)executionResult;
    // ...
}   
```

The Effect property contains a list of Transforms that modify the global state due to the transaction execution. Note that the ExecutionEffect type in Casper v1.x, which also contained a list of operations in addition to the transforms, has been removed in Casper 2.0 execution results.

## Auction contract

The auction contract has also changed in Casper 2.0. If your application tracks validator bids, rewards, and delegators, you must rework how network responses are parsed and interpreted. A complete description of the changes cannot be covered in this guide.

## Server Sent Events

The ServerEventsClient class listens to an event stream from a v1.x node and from v2.x. By default, the client class expects a 2.x network. To listen to a 1.x network change the `nodeVersion` property to `1`. 

### Blocks

The BlockedAdded event contains a Block object. The original block structure, BlockV1 or BlockV2, can be obtained with the cast operator, as described above.

### Transactions

On Casper 2.0, the `DeployAccepted`, `DeployProcessed`, and `DeployExpired` events are replaced with the equivalent `TransactionAccepted`, `TransactionProcessed`, and `TransactionExpired` events. These are emitted for both `Deploy` and `TransactionV1` types of transactions.

A `TransactionAccepted` event contains a `Transaction` property with either a `Deploy` or a `TransactionV1`. `TransactionProcessed` and `TransactionExpired` events contain a `TransactionHash` property with either a deploy hash or a transaction version 1 hash.

### Finality signatures

The `FinalitySignature` event contains an instance of the versioned `FinalitySignature` class. Version 2 of this type is an extension that contains all properties in version 1 plus the block height and the chain name hash.

The user can obtain the original structure by casting this instance to the appropriate version:

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

For a Casper 2.0 network, the `GetNodeStatus` method can be used to get the latest switch block hash. The response contains the `LatestSwitchBlockHash` property with this value.

Also, for blocks produced in Casper 2.0, the `Block` instance contains the previous switch block hash in the `LastSwitchBlockHash` property.

### Checksums (CEP-57)

On SDK v3 only public keys are checksummed with the CEP-57 standard. The rest of the keys and hashes are not checksummed anymore.

