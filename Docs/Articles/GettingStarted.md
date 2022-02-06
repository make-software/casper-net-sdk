# Getting started

Get started with "Casper.Network.SDK" quickly by adding a reference to the NuGet package or linking to the source code project. Or start developing in a remote environment with a Gitpod workspace.

## Add the NuGet package to your project

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

## Add a reference to the source code

Clone the GitHub repository in your solution folder and add a reference in your project to the Casper.Network.SDK.csproj file. 

## Create a Gitpod workspace

Click the button below to start a workspace in Gitpod with a clone of the SDK repository:

[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/make-software/casper-net-sdk)

Once the workspace is ready, go to the terminal and create a new console app:

```
dotnet new console -o GetStarted
```

Now, add a reference to the SDK:

```
cd GetStarted
dotnet add reference ../Casper.Network.SDK/Casper.Network.SDK.csproj
```

Copy the code in one of the examples from `Docs\Examples` and run the program:

```
dotnet run
```
