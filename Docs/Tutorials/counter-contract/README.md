# Counter tutorial with C# and Casper .NET SDK

This example is based on the [Counter contract tutorial](https://casper.network/docs/counter) available in the Casper Network web site.

Using the Casper .NET SDK, we'll show you how to: 

- Send $CSPR from one account to another.
- Deploy the counter contract example to the blockchain.
- Read the counter value.
- Call the `counter_inc` contract entry point to increase the counter.

NOTE: We use a local network in this example. Learn [here](https://casper.network/docs/dapp-dev-guide/setup-nctl) 
how to install your own local network. Alternatively, you can easily adapt the code in this example 
to use the casper-test network. In this case, skip the transfer from faucet step.

### Step 1. Get a new instance of the Casper client

Prepare an instance of the client and a couple of keys that will be used during the example:

```csharp
static string nodeAddress = "http://207.154.217.11:11101/rpc";
static string chainName = "casper-net-1";

static NetCasperClient casperSdk = new NetCasperClient(nodeAddress);

static KeyPair faucetAcct = KeyPair.FromPem("/tmp/faucetact_sk.pem");

static KeyPair myAccount = KeyPair.FromPem("/tmp/myaccount_sk.pem");
static PublicKey myAccountPK = PublicKey.FromPem("/tmp/myaccount_pk.pem");
```

### Step 2. Send $CSPR from the faucet account to `myAccount`

Use the `StandardTransfer` template to create a deploy object that orders a transfer of 2500 $CSPR from the faucet account. Sign it and send it to the network.

Take the `deploy_hash` from the response and make a call to `GetDeploy` with a timeout of 120 seconds. This will query the network until the transfer is complete.

```csharp
public static async Task FundAccount()
{
  var deploy = DeployTemplates.StandardTransfer(
      faucetAcct.PublicKey,
      myAccountPK,
      2500_000_000_000,
      100_000_000,
      chainName);
  deploy.Sign(faucetAcct);
  
  var putResponse = await casperSdk.PutDeploy(deploy);
  
  var deployHash = putResponse.GetDeployHash();
  
  var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
  var getResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);
  
  var execResult = getResponse.Parse().ExecutionResults.First();
  Console.WriteLine("Deploy COST : " + execResult.Cost);
}
```

### Step 3. Deploy the counter contract

If you haven't done yet, get a copy of the repository and build the contract following the instructions in the README file.

```
git clone https://github.com/casper-ecosystem/counter
cd counter
make build-contract
```

As a result you will get the contract compiled at `target/wasm32-unknown-unknown/release/`. Copy the `contract-define.wasm` to your working directory.

Next, use the `ContractDeploy` deploy template to prepare a `Deploy` object. Sign it and deploy it.

```csharp
public static async Task DeployContract(string wasmFile)
{
  var wasmBytes = await File.ReadAllBytesAsync(wasmFile);
  
  var deploy = DeployTemplates.ContractDeploy(
      wasmBytes, 
      myAccount,
      50_000_000_000, 
      chainName);
  deploy.Sign(myAccount);
  
  var putResponse = await casperSdk.PutDeploy(deploy);
  
  var deployHash = putResponse.GetDeployHash();
  
  var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
  var getResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);
  
  var execResult = getResponse.Parse().ExecutionResults.First();
  
  Console.WriteLine("Deploy COST : " + execResult.Cost);
  Console.WriteLine("Contract key: " + execResult.Effect.Transforms.First(t =>
      t.Type == TransformType.WriteContract).Key);
}
```


### Step 4. Get the counter value

The deploy of the contract creates a `Named Key` in the caller account named `counter`. Its key value is the new 
contract hash. 

The contract itself also has a `Named Key` called `count` that stores the value of the counter.

Combining both, we form the path `counter/count`. We can send the `QueryGlobalState` method to the network to get 
the current value of the counter.

```csharp
public static async Task QueryState()
{
  var accountKey = new AccountHashKey(myAccountPK);
  var rpcResponse = await casperSdk.QueryGlobalState(accountKey, null,
     "counter/count");
 
  var result = rpcResponse.Parse();
  Console.WriteLine("Counter value: " + (int)result.StoredValue.CLValue);
}
```

The response from the network will look like the following:

```
{
  "api_version": "1.0.0",
  "stored_value": {
    "CLValue": {
      "cl_type": "I32",
      "bytes": "00000000",
      "parsed": 0
    }
  },
  "merkle_proof": "..."
}  
```

When you know the type of a `CLValue` you can easily get its value with the corresponding 
cast operator. In this case, we know the counter is an integer so we can convert the `CLValue` 
to an integer number with `(int)`.

In the previous code we queried the network starting from an account key and using a path that
hops from the account to the contract and then retrieves the value of the `count` named key. 
But you could also query directly the contract using the contract hash:

```csharp
var contrachHash = "hash-4d6b463f4a5503a1d2fb8dbca92260e3574ff86d68d2103b6d4ecb28246dfb5c";
var response = await casperSdk.QueryGlobalState(contrachHash, null, "count");

var result = response.Parse();
Console.WriteLine("Counter value: " + result.StoredValue.CLValue.ToInt32());
```

### Step 5. Increment the counter

To increment the counter we need to send a deploy and call the `counter_inc` entry point in the 
contract. For this, we can use the `ContractCall` template:

```csharp
public static async Task CallCounterInc()
{
  var deploy = DeployTemplates.ContractCall(
      "counter",
      "counter_inc",
      null,
      myAccountPK,
      15_000_000,
      chainName);
  deploy.Sign(myAccount);
  
  var putResponse = await casperSdk.PutDeploy(deploy);
  
  var deployHash = putResponse.GetDeployHash();
  
  var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
  var getResponse = await casperSdk.GetDeploy(deployHash, tokenSource.Token);
  
  var execResult = getResponse.Parse().ExecutionResults.First();
  
  Console.WriteLine("Deploy COST : " + execResult.Cost);
}
```

### Step 6. Check the counter has a new value

Calling the `QueryState()` method in step 4 we'll now see that the counter has a new value:

```
{
  "api_version": "1.0.0",
  "stored_value": {
    "CLValue": {
      "cl_type": "I32",
      "bytes": "01000000",
      "parsed": 1
    }
  },
  "merkle_proof": "0300000000989ca079a..."
}  
```

The git repository also contains a second contract named `counter-call`. It is an example of how to increment the counter value with a deploy instead of calling a contract entry point.

### Logging handler

The example in `Program.cs` creates the Casper client passing a logging handler as a reference. This is useful to see the communication exchanged with the node. 

To print the logs to the standard output, use:

```
LoggerStream = new StreamWriter(Console.OpenStandardOutput())
```

To print the logs to a file, use:

```
LoggerStream = File.AppendText("netcasper.log")
```
