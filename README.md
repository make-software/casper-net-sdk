# Casper .NET SDK

![build-and-test workflow](https://github.com/make-software/casper-net-sdk/actions/workflows/build-and-test.yml/badge.svg)

The Casper .NET SDK allows developers to interact with the Casper Network using the .NET languages.

**Casper .NET SDK GitHub repository**

This repository. It contains the source code to build the Casper .NET SDK library along with the Unit Test project.

## Build/Test instructions

To build this library, install .NET 5.0 or higher and build with command:

```
dotnet build --configuration Release
```

To run the tests, use this command:

```
dotnet test --settings NetCasperTest/test.runsettings
```

## Usage Examples:

* [Counter contract tutorial with C#](https://hackmd.io/@K48d9TN9T2q7ERX4H27ysw/SJBnPCdVt)
* [Key-Value storage tutorial with C#](https://hackmd.io/@K48d9TN9T2q7ERX4H27ysw/HyX8i0WBt)
* [Key management](https://hackmd.io/@K48d9TN9T2q7ERX4H27ysw/HkvV-MMBt)


## TODO

* Implement proper typing for all RPC responses
* Implement Casper Node 1.4.x RPC changes/additions
* Use native C# types and data structures to create deploy arguments and use CLValues for non-native types only.
* Provide native documentation.
* Review compatibility with previous versions of .NET.
* Increase test coverage with new Unit Tests.
* Implement new deploy templates for more complex use cases.
