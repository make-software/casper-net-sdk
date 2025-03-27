# Changelog

All notable changes to this project will be documented in this file.  The format is based on [Keep a Changelog].

[comment]: <> (Added:      new features)
[comment]: <> (Changed:    changes in existing functionality)
[comment]: <> (Deprecated: soon-to-be removed features)
[comment]: <> (Removed:    now removed features)
[comment]: <> (Fixed:      any bug fixes)
[comment]: <> (Security:   in case of vulnerabilities)

## [3.0.0]

This version is identical to `3.0.0-beta4`. Please review the changes in the previous beta releases, and refer to the [Migration guide](https://github.com/make-software/casper-net-sdk/blob/master/Docs/Articles/Casper20MigrationGuide.md) if you’re updating a project from `v2.x`.

## [3.0.0-beta4]

### Added

* Added the `Unbond` bid kind to the `Bids` list in the get auction info response.

## [3.0.0-beta3]

### Added

* The `TransactionBuilder` now accepts amount as `ulong` type for the `Payment()` method.
* New `GlobalStateKey.FromValidatorKey()` to get the bid address key of a validator.
* New `GetNodeVersion()` in the main client class as a convenient method to check node version.
* New `GetValidatorBid()` method to recover the bid information of a validator.

### Changed

* `MessageKey` contains an `AddressableEntity` property instead of a `HashAddr` to represent the related contract entity.
* `GetAuctionInfo()` now uses the new `state_get_auction_info_v2` RPC method in Casper 2.0.

### Fixed

* [#99](https://github.com/make-software/casper-net-sdk/issues/99) SSE Listener emits the same event 3 times when listening to EventType.All
* Fixed the `TransactionV1.ValidateHashes()` method to properly verify the hashes.

## [3.0.0-beta2]

### Added

* Added `GetDelegatorReward` and `GetValidatorReward` methods to the RPC client. [PR#76](https://github.com/make-software/casper-net-sdk/pull/76) and [PR#80](https://github.com/make-software/casper-net-sdk/pull/80).
* Added `GetStatePackage` method to the RPC client. [PR#82](https://github.com/make-software/casper-net-sdk/pull/82).
* Added `ProtocolVersion` to Get Node Status RPC response. [PR#77](https://github.com/make-software/casper-net-sdk/pull/77).
* Added a `TransactionBuilder` to build `TransactionV1` transactions for the different type of invocations ( to mint/auction system contracts, user stored contracts) and session deployments. [PR#79](https://github.com/make-software/casper-net-sdk/pull/79).

### Changed

* The `Transaction` type is now an abstraction for `TransactionV1` and `Deploy` transaction models. [PR#79](https://github.com/make-software/casper-net-sdk/pull/79).
* Updated `TransactionV1` JSON and bytes serialization in alignment to `casper-node v2.0.0-rc5`. [PR-84](https://github.com/make-software/casper-net-sdk/pull/84) and [PR-85](https://github.com/make-software/casper-net-sdk/pull/85)
* Updated `MessageKey` key type in alignment to `casper-node v2.0.0-rc5`. [PR-86](https://github.com/make-software/casper-net-sdk/pull/86)
* `EntryPointPayment.SelfOnly` constant replaced with `EntryPointPayment.DirectInvocationOnly`.[PR#87](https://github.com/make-software/casper-net-sdk/pull/87).

### Fixed

* Fixed a multi-thread bug when using the RPCLoggingHandler in the RPC client class. [PR#81](https://github.com/make-software/casper-net-sdk/pull/81).
* v3.0.0-beta1 did not parse correctly `Step` event emmitted by nodes in Casper v1.x. [PR#78](https://github.com/make-software/casper-net-sdk/pull/78).

## [3.0.0-beta1]

This version is compatible with Casper node v2.0.0-rc3 and Casper node v1.5.6.

### Added

* New `GetEntity()` method added to the RPC client.
* New `QueryBalanceDetails()` method added to the RPC client.
* New `PutTransaction()` and `GetTransaction()` methods added to the RPC client.
* New `TransactionV1` class to model the new type of transactions in Condor.
* New global state keys added to the SDK: `AddressableEntityKey`, `BalanceHoldKey`, `BidAddrKey`, `BlockGlobalAddrKey`, `ByteCodeKey`, `EntryPointKey`, `MessageKey`, `NamedKeyKey` and `PackageKey`.
* New `AddressableEntity` class added. It may represent an account, a stored smart contract, or a system smart contract.
* New properties in the `StoredValue` class that can be retrieved with `QueryGlobalState()` method: `BidKind`, `Unbonding`, `AddressableEntity`, `Package`, `ByteCode`, `MessageTopicSummary`, `Message`, `NamedKey`, `Reservation`, and `EntryPoint`.
* New classes to represent data from the global state: `BidKind`, `UnbondingPurse`, `Package` and `ByteCode`.
* New `Message` class to contain data for native events included in the `TransactionAccepted` event from the SSE channel.
* Added `TransactionAccepted`, `TransactionProcessed`, and `TransactionExpired` events to the list of events emitted by a node through the SSE interface.

### Changed

* The `Block` class has changed to abstract the developer from the versioned block records returned from the network in Condor version. Refer to the migration guide for more information. 
* For blocks produced in Casper 2.0, the `Block` instance contains the previous switch block hash in the `LastSwitchBlockHash` property.
* Ther `EraEnd` class has changed to abstract the developer from the versioned records returned from the network. More info in the migration guide. 
* The `Transfer` class contained in the response for a `GetBlockTransfers()` call or in a `ExecutionResult` object has changed to abstract from the versioned transfer records returned from the network. Refer to the migration guide for more information.
* The input argument for the `QueryBalance()` method in the RPC client is any object from a class implementing the `IPurseIdentifier` interface. `PublicKey`, `AccountHashKey`, `URef`, and `AddressableEntity` classes implement this interface.
* The type to refer to block heights is now `ulong` across all the SDK. In previous version there was a mix of `ulong` and `int`. 
* When using the `GetNodeStatus()` method with a Casper 2.0 node, the response contains the hash for the latest switch block in the `LatestSwitchBlockHash` property.
* `GetDeploy()` response has changed and now contains a `ExecutionInfo`object when the deploy has been processed instead a list of `ExecutionResult` objects. The execution info itself contains block information and a result object. 
* Starting with this version of the SDK, only public keys are checksummed with the CEP-57 standard. The rest of the keys and hashes are not checksummed anymore. 
* In the `StoredValue` class, `Transfer` has been renamed to `LegacyTransfer`.
* `DeployApproval` class has been renamed to `Approval` and is used for `Deploy` as well as for the new `TransactionV1` model. 
* `ActionThresholds` class has now new `UpgradeManagement` property.
* The `EntryPoint` class has new entry point types and a new property `EntryPointPayment`. Both apply when working with Casper 2.0 only.
* `Step` event from SSE contains a list of `Transform` objects instead of a `ExecutionEffect` instance.
* `FinalitySignature` event contains `BlockHeight` and `ChainNameHash` value when connected to a Casper 2.0 node. 
* `DeployProcessed` event from SSE contains a `ExecutionResultV1` object instead of a `ExecutionResult` object.

### Deprecated

* `Proposer.isSystem` property is marked as Obsolete. Use `IsSystem` with capital `I` instead.

### Removed

* `GetAccountBalance()` method in the RPC client has been removed. Use `QueryBalance()` instead. `GetBalance()` method exists to use `state_get_balance` from the RPC interface if needed.

### Security

* BouncyCastle package updated to 2.4.0 version.

## [2.4.0]

### Changed

* Upgraded the project to .NET 8 (see Security section).

### Security

* Addressed [CVE-2024-30105](https://github.com/dotnet/runtime/issues/104619) by upgrading the project to .NET 8.
* Updated BouncyCastle.Cryptography dependency to `v2.4.0` to address a vulnerability in `v2.3.0`.

## [2.3.0]

### Added
* Compatibility with `netstandard2.0` framework. 
* Multi-framework nuget package.

## [2.2.0]

### Added
* Changes to QueryGlobalState and GetAccountInfo methods to support changes in Casper node v1.5.5 [PR#50](https://github.com/make-software/casper-net-sdk/pull/50).

### Fixed
* We use `ToLowerInvariant()` instead of `ToLower()` to avoid problems with some CultureInfos [PR#52](https://github.com/make-software/casper-net-sdk/pull/52).
* XML Documentation included into the nuget packages [PR#53](https://github.com/make-software/casper-net-sdk/pull/53).
* `isSystem` property in the Block Proposer class marked obsolete and replaced with `IsSystem` [PR#51](https://github.com/make-software/casper-net-sdk/pull/51).

### Changed
* The SDK now links with `BouncyCastle.Cryptography` package instead of `BouncyCastle.NetCore` which was based on a 
mirror of the official package.
* Testnet node in the examples updated to a working URL.

## [2.1.0]

### Fixed
* Breaking change! the type for the `Proposer` property in the body block has changed from `PublicKey` to `Proposer`.
This new type permits to parse correctly the value `"00"` used for system blocks in Casper network starting from v1.5.

## [2.0.0]

### Added
* Added client method for new `query_balance` RPC endpoint.
* Added client method for new `speculative_exec` feature in casper node v1.5.0.
* Added client method for new `info_get_chainspec` RPC endpoint.
* Added client method for new `chain_get_era_summary` RPC endpoint.
* Added support for `block_height` parameter in `query_global_state` RPC endpoint.
* Added new key types: `chainspec-registry-`, `system-contract-registry-`, `checksum-registry-`, `era-summary`, `unbond-`.
* Added support for new key prefix 'contract-package-'.
* Added new fields related to node sync in `info_get_status` RPC response.
* Added support optional fields in `info_get_deploy` RPC response.
* Added `lock_status` field to `ContractPackageStatus`.

### Changed
* The SDK now builds with .NET 7 Framework.
* 
## [1.1.2]

### Added
* New RPC method `GetEraSummary()`.
* New global state key `era-summary-0000...0000`.

## [1.1.1]

### Fixed
* Skip `block_identifier` param if `Hash` value is `null` in RPC requests to be compatible with Casper node v150.

## [1.1.0]

### Added
* New in casper-node 1.4.5. Added optional `finalized_approvals` parameter to `GetDeploy` method.
* `CLValue` class offers a new method `Some()` to extract values wrapped in the `Option(CLType)` type.

### Changed
* Removed the dependency with the external library Humanizer.
* `NetCasperClient` implements `ICasperClient`. Similarly, `ServerEventsClient` implements `ISSEClient`. 

### Fixed
* Replace `Thread.Sleep()` with `Task.Delay()` to run asynchronously.
* Call await instead of Result in `GetAccountBalance` RPC method to run asynchronously.

## [1.0.0]

### Added
* Initial release of Casper .NET SDK.

[3.0.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v3.0.0
[3.0.0-beta4]: https://github.com/make-software/casper-net-sdk/releases/tag/v3.0.0-beta4
[3.0.0-beta3]: https://github.com/make-software/casper-net-sdk/releases/tag/v3.0.0-beta3
[3.0.0-beta2]: https://github.com/make-software/casper-net-sdk/releases/tag/v3.0.0-beta2
[3.0.0-beta1]: https://github.com/make-software/casper-net-sdk/releases/tag/v3.0.0-beta1
[2.3.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v2.3.0
[2.2.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v2.2.0
[2.1.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v2.1.0
[2.0.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v2.0.0
[1.1.2]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.2
[1.1.1]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.1
[1.1.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.0
[1.0.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.0.0
