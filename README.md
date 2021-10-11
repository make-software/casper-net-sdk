# NetCasperSDK - C# SDK for Casper network

[build-and-test workflow](https://github.com/davidatwhiletrue/netcaspersdk/actions/workflows/build-and-test.yml/badge.svg)

## Friendly Hackaton
My work submission to the *Building An SDK In C#* bounty consist of:

* This repository containing a C# SDK to connect to a Casper node. 
* A pull request in GitHub `casper-integrations` repository with examples of usage of the SDK.
* A pull request in GitHub `casper-network/docs` repository to list the NetCasperSDK in [SDK Client Libraries](https://docs.casperlabs.io/en/latest/dapp-dev-guide/sdk/index.html) page.

**NetCasperSDK GitHub repository**

This repository. It contains the source code to build the NetCasperSDK library along with the Unit Test project.


**Pull Request in GitHub `casper-integrations` repository**

https://github.com/casper-network/casper-integrations/pull/7

Contains examples for different use cases of interaction with a node using the NetCasperSDK. In addition to all the existing `get-*` examples, I've added an example to make a transfer and different examples of the SSE client to listen to events from a node.

**Pull Request in GitHub `casper-network/docs` repository** 

https://github.com/casper-network/docs/pull/365

This PR adds a link to this repository to the *SDK Client Libraries* page. It also includes a subpage with simple build/test instructions and links to examples.

**Future of NetCasperSDK**

This is just the beginning. I'd like to continue working on this SDK, along with other .NET developers that want to join, and improve it with new functionalities. In my roadmap I have:

* Support to Blazor projects to easily build web applications which connect to Casper.
* Review compatibility with previous versions of .NET.
* Integration with the Casper Signer extension for Chrome.
* Measure the test coverage and increase with new Unit tests.
* Implement new deploy templates for more complex use cases.

Also, the repository could be transferred to an official GitHub account if the organization would like to.

## Build/Test instructions

Build and test instructions will be added to this page when the hackaton is finished. For the time being, check the link below:

https://github.com/davidatwhiletrue/docs/blob/hackaton-netcaspersdk/dapp-dev-guide/sdk/csharp-sdk.rst
 
## Examples:

* [Counter contract tutorial with C#](https://hackmd.io/@K48d9TN9T2q7ERX4H27ysw/SJBnPCdVt)
* [Key-Value storage tutorial with C#](https://hackmd.io/@K48d9TN9T2q7ERX4H27ysw/HyX8i0WBt)
* [Key management](https://hackmd.io/@K48d9TN9T2q7ERX4H27ysw/HkvV-MMBt)

