# Migration from Casper .NET SDK v2.x to v3.0

To migrate your application from Casper .NET SDK v2.x to Casper .NET SDK v3.0 you'll need to make several
changes on your code. The good news is that with version 3 of the SDK your application will be compatible with
Casper v1.5.6 as well as with Casper v2.0 (aka Condor).

## Types

### Blocks

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

#### Recovering the versioned block object

If, for any reason, you need to work with the original format of the block, you can cast a `Block` to a `BlockV1`
or `BlockV2`. For example:

```csharp
if (block.Version == 2) {
    var blockv2 = (BlockV2)block;
    // ...
} else if (blockVersion == 1) {
    var blockv1 = (BlockV1)block;
    // ...
}    
```

#### EraEnd

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

#### Transactions

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

#### Block height type

The type to represent block height is now `ulong` everywhere. `int` was used in some methods or types in the previous
version. That's not the case with v3.

## Other changes

### Checksums (CEP-57)

On SDK v3 only public keys are checksummed. The rest of keys and hashes are not checksummed anymore.
