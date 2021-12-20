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
dotnet test --settings Casper.Network.SDK.Test/test.runsettings
```

### Create a workspace in Gitpod

Click the button to start coding in Gitpod with an online IDE.

[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/make-software/casper-net-sdk)

## Usage Examples

* [Counter contract tutorial with C#](./Docs/Tutorials/Counter%20Contract/README.md)
* [Key-Value storage tutorial with C#](./Docs/Tutorials/KVStorage%20Contract/README.md)
* [Key management](./Docs/KeyManagement.md)
* [Working with CLValue](./Docs/WorkingWithCLValue.md)

## TODO

* Provide native documentation.
* Review compatibility with previous versions of .NET.
* Increase test coverage with new Unit Tests.
* Implement new deploy templates for more complex use cases.
