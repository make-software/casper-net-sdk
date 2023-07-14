# Changelog

All notable changes to this project will be documented in this file.  The format is based on [Keep a Changelog].

[comment]: <> (Added:      new features)
[comment]: <> (Changed:    changes in existing functionality)
[comment]: <> (Deprecated: soon-to-be removed features)
[comment]: <> (Removed:    now removed features)
[comment]: <> (Fixed:      any bug fixes)
[comment]: <> (Security:   in case of vulnerabilities)

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

[1.1.2]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.2
[1.1.1]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.1
[1.1.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.1.0
[1.0.0]: https://github.com/make-software/casper-net-sdk/releases/tag/v1.0.0
