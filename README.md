# Casper .NET SDK

[![GitHub version](https://badge.fury.io/gh/make-software%2Fcasper-net-sdk.svg)](https://badge.fury.io/gh/make-software%2Fcasper-net-sdk)  [![NuGet version](https://badge.fury.io/nu/casper.network.sdk.svg)](https://badge.fury.io/nu/casper.network.sdk)

![build-and-test workflow](https://github.com/make-software/casper-net-sdk/actions/workflows/build-and-test.yml/badge.svg)

The Casper .NET SDK allows developers to interact with the Casper Network using the .NET languages. The project itself is being developed with C#.

## Documentation

The SDK documentation, examples and tutorials can be found [here](https://make-software.github.io/casper-net-sdk/).

## Get started

The Casper.Network.SDK for .NET is published as a nuget package in [nuget.org](https://www.nuget.org/packages/Casper.Network.SDK).

To add a reference to the SDK in your project, use the Package Manager in Visual Studio or the `dotnet` cli tool.

### Package Manager (Windows)
```
Install-Package Casper.Network.SDK
``` 

### dotnet cli tool (Windows/Mac/Linux)
```
dotnet add package Casper.Network.SDK
``` 

## Run a Casper node locally with NCTL

[NCTL](https://github.com/casper-network/casper-node/tree/release-1.4.3/utils/nctl) is a CLI application to control one or multiple Casper networks locally. Many developers wish to spin up relatively small test networks to localize their testing before deploying to the blockchain.

To simplify even more the set up of a local network, you may run NCTL within a docker container. To start a container and publish the ports of one the nodes, write the following command:

```bash
docker run --rm -it --name mynctl -d -p 11101:11101 -p 14101:14101 -p 18101:18101 makesoftware/casper-nctl
```

Refer to the [`casper-nctl-docker`](https://github.com/make-software/casper-nctl-docker/) repository for further details on how to use NCTL with docker.

## Build/Test instructions

To build this library, install .NET 5.0 or higher and build with command:

```
dotnet build --configuration Release
```

To run the tests, use this command:

```
dotnet test --filter 'TestCategory!~NCTL'
```

On Windows use a PowerShell terminal to run the tests.

To test against `netstandard2.0` framework, launch the tests as follows:

```
TEST_FRAMEWORK=netstandard2.0 dotnet test --filter 'TestCategory!~NCTL'
```

### Integration tests

The command above excludes integration tests. If you're running a Casper network locally with NCTL, follow these steps to run the integrations tests:

1. Copy the faucet key from your NCTL environment to `Casper.Network.SDK.Test/TestData/faucetact.pem`. If you're running the NCTL docker image, activate nctl commands and run `nctl-view-faucet-secret-key > Casper.Network.SDK.Test/TestData/faucetact.pem`.

2. Adjust, if needed, the IPs and ports in the file `Casper.Network.SDK.Test/TestData/test.runsettings`.

3. Run the tests:

```
dotnet test --settings Casper.Network.SDK.Test/test.runsettings --filter 'TestCategory~NCTL'
```

NOTE: Make sure your NCTL network has booted up and nodes are emitting blocks before running the tests.

To test against `netstandard2.0` framework, launch the tests as follows:

```
TEST_FRAMEWORK=netstandard2.0 dotnet test --settings Casper.Network.SDK.Test/test.runsettings --filter 'TestCategory~NCTL'
```

## Create a workspace in Gitpod

Click the button to start coding in Gitpod with an online IDE.

[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/make-software/casper-net-sdk)
