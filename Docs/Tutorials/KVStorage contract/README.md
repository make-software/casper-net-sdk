# Key-Value contract tutorial with C# and Casper .NET SDK

This example is based on the [Key-Value contract tutorial](https://casper.network/docs/dapp-dev-guide/tutorials/kv-storage-tutorial) 
available in the Casper Network web site.

Using the Casper .NET SDK, we'll show you how to:

- Deploy the key-value contract example to the blockchain.
- Call an entry point in the contract to store a value with a given named key.
- Read the previously stored value. 

Read first the [Counter tutorial](../Counter%20Contract/README.md) to prepare your environment and one account with
enough $CSPR to make deploys.

NOTE: We use a local network in this example. Learn [here](https://casper.network/docs/dapp-dev-guide/setup-nctl) 
how to install your own local network. Alternatively, you can easily adapt the code in this example to use the casper-test 
network.

### Step 1. Deploy the key-value storage contract

Clone the GitHub repository and build the contract following the instructions in the README file.

```
git clone https://github.com/casper-ecosystem/kv-storage-contract
cd kv-storage-contract
make build-contract
```

As a result you will get the contract compiled at `target/wasm32-unknown-unknown/release/`. Copy the `contract.wasm` file
to your working directory.

Next, use the `ContractDeploy` deploy template to prepare a `Deploy` object. Sign it and send it to the network.

```csharp
public static async Task<HashKey> DeployKVStorageContract()
{
    var wasmFile = "./contract.wasm";
    var wasmBytes = System.IO.File.ReadAllBytes(wasmFile);
    
    var deploy = DeployTemplates.ContractDeploy(
        wasmBytes, 
        myAccountPK,
        300_000_000_000, 
        chainName);
    deploy.Sign(myAccount);

    var response = await casperSdk.PutDeploy(deploy);

    string deployHash = response.GetDeployHash();
    
    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
    var deployResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

    var execResult = deployResponse.Parse().ExecutionResults.First();

    Console.WriteLine("Deploy COST : " + execResult.Cost);
    
    var contractHash = execResult.Effect.Transforms.First(t =>
                t.Type == TransformType.WriteContract).Key;
    Console.WriteLine("Contract key: " + contractHash);
    
    return (HashKey)contractHash;
}
```

The deployment of the contract has added to the account a couple of named keys. We can retrieve our 
account information to see them:

````csharp
public async static Task GetAccountInfo()
{
    var response = await casperSdk.GetAccountInfo(myAccountPK);
    
    File.WriteAllText("res_GetAccountInfo.json", response.Result.GetRawText());
}
````

In the response we can identify the named key `kvstorage_contract`. It contains the hash of the contract. 
We'll use it in a later step to call the contract to store a value.  

Print a beautified json using `jq` like this:
```
cat res_GetAccountInfo.json | jq
```

```
{
  "api_version": "1.0.0",
  "stored_value": {
    "Account": {
      "account_hash": "account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7",
      "named_keys": [
        {
          "name": "kvstorage_contract",
          "key": "hash-0aaab9926971ac1564d6057d269aff4b93e621c788e2cfc2e9fbe48fac345285"
        },
        {
          "name": "kvstorage_contract_hash",
          "key": "uref-eafede874c50f50841e589825d41a6ff9c2abe12434f52d9a85816379f1acefd-007"
        }
      ],
      "main_purse": "uref-ea6dd32086d7878460a042357c8332d2d19251bb21f5ad809fc048855e167e9b-007",
      "associated_keys": [
        {
          "account_hash": "account-hash-989ca079a5e446071866331468ab949483162588d57ec13ba6bb051f1e15f8b7",
          "weight": 1
        }
      ],
      "action_thresholds": {
        "deployment": 1,
        "key_management": 1
      }
    }
  },
  "merkle_proof": "010..."
}
```

### Step 2. Store an integer value in the contract named keys

We can call now the contract and store a key-value pair. In the code below, we use the `ContractCall` deploy template
to call the `store_i32` entry point in the `kvstorage_contract`.

```csharp
public async static Task StoreI32()
{
    var namedArgs = new List<NamedArg>()
    {
        new NamedArg("name", "I32MinValue"),
        new NamedArg("value", int.MinValue)
    };

    await StoreKeyValue("store_i32", namedArgs, "res_StoreI32.json");
}

private async static Task StoreKeyValue(string entryPoint, List<NamedArg> namedArgs, string saveToFile)
{
    var deploy = DeployTemplates.ContractCall("kvstorage_contract",
        entryPoint,
        namedArgs,
        myAccountPK,
        1_000_000_000,
        chainName);
    deploy.Sign(myAccount);

    var response = await casperSdk.PutDeploy(deploy);
    var deployHash = response.GetDeployHash();

    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
    var deployResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

    if (saveToFile != null)
        File.WriteAllText(saveToFile, deployResponse.Result.GetRawText());
}
```

Note that for this deploy we're paying 1 $CSPR. Check in the json response how much the actual cost is:

```
cat res_StoreI32.json | jq | grep "cost"
```

### Step 3. Read the number stored

Combining the name of the contract and the named key we can form a path and query the network to get the value:

```csharp
public async static Task<CLValue> ReadStoredValue(string namedKey)
{
    var accountKey = new AccountHashKey(myAccountPK);
    var rpcResponse = await casperSdk.QueryGlobalState(accountKey, null,
        $"kvstorage_contract/{namedKey}");

    Console.WriteLine(rpcResponse.Result.GetRawText());
    File.WriteAllText($"res_ReadStoredValue_{namedKey}.json", rpcResponse.Result.GetRawText());

    return rpcResponse.Parse().StoredValue.CLValue;
}

// Main method
//
var clValue = await ReadStoredValue("I32MinValue");
var number = clValue.ToInt32();
Console.WriteLine("Value: " + number);
```

### Step 4. Store a string value calling the contract by its hash key

In the step 2 we stored a value using the contract named key. That works only when the deploy is sent 
by the same account that deployed the contract. To store a value from a different account, we use the 
contract hash key instead:

```csharp
public async static Task StoreString(HashKey contractHash = null)
{
    var namedArgs = new List<NamedArg>()
    {
        new NamedArg("name", "WorkingOn"),
        new NamedArg("value", "Casper .NET SDK")
    };

    var deploy = DeployTemplates.ContractCall(contractHash,
        "store_string",
        namedArgs,
        myAccountPK,
        500_000_000,
        chainName);

    deploy.Sign(myAccount);

    var response = await casperSdk.PutDeploy(deploy);
    var deployHash = response.GetDeployHash();

    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
    var deployResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);

    File.WriteAllText("res_StoreString.json", deployResponse.Result.GetRawText());
}

// Main method
//
await StoreString(contractHash);
``` 

### Step 5. Read the string value using the contract key

Similarly, we can read the stored string with the contract key and the named key:

```
public async static Task<CLValue> ReadStoredValue(string namedKey, GlobalStateKey contractHash)
{
    var queryResponse = await casperSdk.QueryGlobalState(contractHash, null, namedKey);

    File.WriteAllText($"res_ReadStoredValue_{namedKey}.json", queryResponse.Result.GetRawText());

    return queryResponse.Parse().StoredValue.CLValue;
}

// Main method 
//
var clValue = await ReadStoredValue("WorkingOn", contractHash);
Console.WriteLine("Value: " + (string)clValue);
```

Note that in this case we're querying the network without our public key. The public key was needed in step 3 
to recover the contract hash from a named key. Now, we use the contract hash directly.

### Step 6. Store and read different CLValue types

So far, we've stored an integer and a string in the contract named keys. The code in `Program.cs` contains 
methods to store many of the other CLValues. We encourage you to test some of them now.
