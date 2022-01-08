# Casper .NET SDK

![build-and-test workflow](https://github.com/make-software/casper-net-sdk/actions/workflows/build-and-test.yml/badge.svg)

The Casper .NET SDK allows developers to interact with the Casper Network using the .NET languages. The project itself is being developed with C#.

## Build/Test instructions

To build this library, install .NET 5.0 or higher and build with command:

```
dotnet build --configuration Release
```

To run the tests, use this command:

```
dotnet test --filter "TestCategory!~NCTL"
```

### Integration tests

The command above excludes integration tests. If you're running a Casper network locally with NCTL, follow these steps to run the integrations tests:

1. Copy the faucet key from your NCTL environment to `Casper.Network.SDK.Test/TestData/faucetact.pem`.

2. Adjust, if needed, the IPs and ports in the file `Casper.Network.SDK.Test/TestData/test.runsettings`.

3. Run the tests:

```
dotnet test --settings Casper.Network.SDK.Test/test.runsettings --filter "TestCategory~NCTL"
```

## Create a workspace in Gitpod

Click the button to start coding in Gitpod with an online IDE.

[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/make-software/casper-net-sdk)

## Usage Examples (WIP)

* [Counter contract tutorial with C#](./Docs/Tutorials/counter-contract/README.md)
* [Key-Value storage tutorial with C#](./Docs/Tutorials/kvstorage-contract/README.md)
* [Key management](./Docs/Articles/KeyManagement.md )
* [Working with CLValue](./Docs/Articles/WorkingWithCLValue.md)

## TODO

* Review compatibility with previous versions of .NET.
* Implement new deploy templates for more complex use cases.
