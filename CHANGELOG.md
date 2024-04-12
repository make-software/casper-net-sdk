# Changelog

All notable changes to this project will be documented in this file.  The format is based on [Keep a Changelog].

[comment]: <> (Added:      new features)
[comment]: <> (Changed:    changes in existing functionality)
[comment]: <> (Deprecated: soon-to-be removed features)
[comment]: <> (Removed:    now removed features)
[comment]: <> (Fixed:      any bug fixes)
[comment]: <> (Security:   in case of vulnerabilities)

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

[2.2.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v2.2.0
[2.1.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v2.1.0
[2.0.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v2.0.0
[1.1.2]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.2
[1.1.1]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.1
[1.1.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.0
[1.0.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.0.0
