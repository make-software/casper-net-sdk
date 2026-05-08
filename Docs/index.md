---
_disableToc: false
---

# Casper.Network.SDK

[![GitHub version](https://badge.fury.io/gh/make-software%2Fcasper-net-sdk.svg)](https://badge.fury.io/gh/make-software%2Fcasper-net-sdk)  [![NuGet version](https://badge.fury.io/nu/casper.network.sdk.svg)](https://badge.fury.io/nu/casper.network.sdk)

![build-and-test workflow](https://github.com/make-software/casper-net-sdk/actions/workflows/build-and-test.yml/badge.svg)

The Casper .NET SDK allows developers to interact with the Casper Network using .NET languages. The project is developed with C# and targets **.NET 8**.

> **New in v3.x:** This version of the SDK supports both **Casper 1.x** and **Casper 2.0** networks. It introduces `TransactionV1`, a new transaction model for Casper 2.0, alongside the legacy `Deploy` model. If you are upgrading from v2.x, please read the [Migration Guide](./Articles/Casper20MigrationGuide.md).

## Articles

* [Getting started](./Articles/GettingStarted.md) — Install the SDK and run your first program.
* [Working with TransactionV1](./Articles/WorkingWithTransactionV1.md) — Build, sign, and send transactions using the new Casper 2.0 transaction model.
* [Working with CLValue](./Articles/WorkingWithCLValue.md) — Convert between C# types and Casper's `CLValue` type system.
* [Querying Balances](./Articles/QueryingBalances.md) — Retrieve account and purse balances on Casper 2.0 networks.
* [Working with AddressableEntity](./Articles/WorkingWithAddressableEntity.md) — Query entities (accounts, contracts, and system contracts) on Casper 2.0.
* [Key management](./Articles/KeyManagement.md) — Create, load, and save public/private key pairs.
* [Working with Server-Sent Events (SSE)](./Articles/WorkingWithSSE.md) — Listen to real-time events from a Casper node.
* [Casper Event Standard (CES)](./Articles/CasperEventStandard.md) — Decode typed events emitted by smart contracts.
* [Running a local Casper network with NCTL](./Articles/RunningNctlLocally.md) — Set up a local test network using Docker.
* [Migration from v2.x to v3.0](./Articles/Casper20MigrationGuide.md) — Upgrade your application for Casper 2.0 compatibility.

## Usage Examples

See the complete list of examples [here](./Examples/index.md).

## Tutorials

* [Counter contract tutorial with C#](./Tutorials/counter-contract/README.md)
* [Key-Value storage tutorial with C#](./Tutorials/kvstorage-contract/README.md)
* [ERC-20 tutorial with C#](./Tutorials/erc20-contract/README.md)

## API Documentation

* [API Documentation](./api/index.md)

## Links

* [GitHub repository](https://github.com/make-software/casper-net-sdk)
* [NuGet package](https://www.nuget.org/packages/Casper.Network.SDK/)
