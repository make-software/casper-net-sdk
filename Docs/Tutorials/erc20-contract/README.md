# ERC-20 contract tutorial with C# and Casper .NET SDK

This tutorial is a C# version of the [ERC-20 contract tutorial](https://casper.network/docs/erc20) 
available on the Casper Network website. You're encouraged to read that tutorial first to get acquainted with 
the implementation of the ERC-20 standard for the Casper blockchain.

In this document, we'll show you how to use the Casper .NET SDK to:

- Deploy the ERC-20 contract example to the blockchain.
- Transfer tokens to an account and get the balance of tokens.
- Call the Approve function to allow a spender to transfer tokens from the owner account. 

Read first the [Counter tutorial](../counter-contract/README.md) to prepare your environment and one account with 
enough $CSPR to make deploys.

NOTE: We use a local network with NCTL in this example. Learn [here](https://casper.network/docs/dapp-dev-guide/setup-nctl) 
how to install your local network. Alternatively, you can easily adapt the code in this example to use the casper-test 
network.

### Step 1. Deploy the key-value storage contract

Clone the GitHub repository and build the contract following the instructions in the README file.

```
git clone https://github.com/casper-ecosystem/erc20 
cd erc20
make prepare
make build-contracts
```

As a result, you will get the contract compiled at `target/wasm32-unknown-unknown/release/`. Copy the `erc20_token.wasm` file
to your working directory.

We used the `ContractDeploy` deploy template to prepare a `Deploy` object in other examples. To show what's behind that template, 
we're showing here all the steps needed to prepare the deploy and send it to the network.

```csharp
public static async Task<HashKey> DeployERC20Contract(KeyPair accountKey)
{
    var wasmFile = "./erc20_token.wasm";
    var wasmBytes = System.IO.File.ReadAllBytes(wasmFile);

    var header = new DeployHeader()
    {
        Account = accountKey.PublicKey,
        Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
        Ttl = 1800000,
        ChainName = chainName,
        GasPrice = 1
    };
    var payment = new ModuleBytesDeployItem(300_000_000_000);

    List<NamedArg> runtimeArgs = new List<NamedArg>();
    runtimeArgs.Add(new NamedArg("name", "C# SDK Token"));
    runtimeArgs.Add(new NamedArg("symbol", "CSSDK"));
    runtimeArgs.Add(new NamedArg("decimals", (byte) 5)); //u8
    runtimeArgs.Add(new NamedArg("total_supply", CLValue.U256(10_000)));

    var session = new ModuleBytesDeployItem(wasmBytes, runtimeArgs);

    var deploy = new Deploy(header, payment, session);
    deploy.Sign(accountKey);

    await casperSdk.PutDeploy(deploy);

    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
    var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

    var execResult = deployResponse.Parse().ExecutionResults.First();

    Console.WriteLine("Deploy COST : " + execResult.Cost);

    var contractHash = execResult.Effect.Transforms.First(t =>
        t.Type == TransformType.WriteContract).Key;
    Console.WriteLine("Contract key: " + contractHash);

    File.WriteAllText("res_DeployERC20Contract.json", deployResponse.Result.GetRawText());

    return (HashKey) contractHash;
}
```

### Step 2. Read the balance of an account

The account that has deployed the contract owns the total supply of tokens. We can read the balance of an account 
making a query to the `balances` dictionary.

```csharp
public static async Task ReadBalance(string contractHash, PublicKey publicKey)
{
    var accountHash = new AccountHashKey(publicKey);
    var dictItem = Convert.ToBase64String(accountHash.GetBytes());
    
    var response = await casperSdk.GetDictionaryItemByContract(contractHash, "balances", dictItem);

    File.WriteAllText("res_ReadBalance.json", response.Result.GetRawText());

    var result = response.Parse();
    var balance = result.StoredValue.CLValue.ToBigInteger();
    Console.WriteLine("Balance: " + balance.ToString() + " $CSSDK");
}
```

### Step 3. Transfer tokens to another account

To transfer tokens to another account, the owner calls the 'transfer' entry point indicating the recipient and the 
number of tokens to send.

```csharp
public static async Task TransferTokens(string contractHash, KeyPair ownerAccount, PublicKey recipientPk,
            ulong amount)
{
    var deploy = DeployTemplates.ContractCall(new HashKey(contractHash),
        "transfer",
        new List<NamedArg>()
        {
            new NamedArg("recipient", CLValue.Key(new AccountHashKey(recipientPk))),
            new NamedArg("amount", CLValue.U256(amount))
        },
        ownerAccount.PublicKey,
        1_000_000_000,
        chainName);
    deploy.Sign(ownerAccount);

    await casperSdk.PutDeploy(deploy);

    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
    var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

    File.WriteAllText("res_TransferTokens.json", deployResponse.Result.GetRawText());
}
```

### Step 4. Approve an spender to transfer tokens from your account

The `approve` function purpose in the ERC-20 contract is to approve an address to spend tokens on behalf of the 
approver account. In other words, the owner of an account `A` approves that a spender with account `B` can transfer 
tokens from `A`  to any recipient's account (up to a maximum limit).

```csharp
public async static Task ApproveSpender(string contractHash, KeyPair ownerAccount, PublicKey spenderPk, ulong amount)
{
    var deploy = DeployTemplates.ContractCall(new HashKey(contractHash),
        "approve",
        new List<NamedArg>()
        {
            new NamedArg("spender", CLValue.Key(new AccountHashKey(spenderPk))),
            new NamedArg("amount", CLValue.U256(amount))
        },
        ownerAccount.PublicKey,
        1_000_000_000,
        chainName);
    deploy.Sign(ownerAccount);

    await casperSdk.PutDeploy(deploy);

    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
    var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

    File.WriteAllText("res_ApproveSpender.json", deployResponse.Result.GetRawText());
}
```

After approval, the spender has an allowance. This value can be checked by querying the `allowances` dictionary 
of the contract. Since a spender may have allowances from different owners, the key in the dictionary contains 
both the owner and the spender accounts. 

```csharp
public static async Task ReadAllowance(string contractHash, PublicKey ownerPk, PublicKey spenderPk)
{
    var ownerAccHash = new AccountHashKey(ownerPk);
    var spenderAccHash = new AccountHashKey(spenderPk);
    var bytes = new byte[ownerAccHash.GetBytes().Length + spenderAccHash.GetBytes().Length];
    Array.Copy(ownerAccHash.GetBytes(), 0, bytes, 0, ownerAccHash.GetBytes().Length);
    Array.Copy(spenderAccHash.GetBytes(), 0, bytes, ownerAccHash.GetBytes().Length,
        spenderAccHash.GetBytes().Length);

    var bcBl2bdigest = new Org.BouncyCastle.Crypto.Digests.Blake2bDigest(256);
    bcBl2bdigest.BlockUpdate(bytes, 0, bytes.Length);
    var hash = new byte[bcBl2bdigest.GetDigestSize()];
    bcBl2bdigest.DoFinal(hash, 0);

    var dictItem = Hex.ToHexString(hash);

    var response = await casperSdk.GetDictionaryItemByContract(contractHash, "allowances", dictItem);

    File.WriteAllText("res_ReadAllowance.json", response.Result.GetRawText());

    var result = response.Parse();
    var balance = result.StoredValue.CLValue.ToBigInteger();
    Console.WriteLine("Allowance: " + balance.ToString() + " $CSSDK");
}
```

### Step 5. Transfer tokens from the allowance

A spender can transfer tokens from the owner to a recipient using the `transfer_from ` entry point in the ERC-20 contract.

```csharp
public static async Task TransferTokensFromOwner(string contractHash, KeyPair spenderAccount, 
    PublicKey ownerPk, PublicKey recipientPk, ulong amount)
{
    var deploy = DeployTemplates.ContractCall(new HashKey(contractHash),
        "transfer_from",
        new List<NamedArg>()
        {
            new NamedArg("owner", CLValue.Key(new AccountHashKey(ownerPk))),
            new NamedArg("recipient", CLValue.Key(new AccountHashKey(recipientPk))),
            new NamedArg("amount", CLValue.U256(amount))
        },
        spenderAccount.PublicKey,
        1_000_000_000,
        chainName);
    deploy.Sign(spenderAccount);

    await casperSdk.PutDeploy(deploy);

    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
    var deployResponse = await casperSdk.GetDeploy(deploy.Hash, tokenSource.Token);

    File.WriteAllText("res_TransferFromOwner.json", deployResponse.Result.GetRawText());
}
```

The amount of tokens sent to the recipient is deducted from the allowance approved by the owner. You can verify this 
by rereading the allowance with `ReadAllowance()`.

### Step 6. Check the balance or the allowance of any account

When we check the balance or the allowance of an account, we query a dictionary in the contract. Suppose the key that 
identifies an account owner or the pair owner-spender does not exist in the balances or allowances dictionary, 
respectively. Our call to the `GetDictionaryItemByContract` RPC method will throw an error.

To capture this error, wrap the RPC call in a try-catch block.

```csharp
try
{
    var response = await casperSdk.GetDictionaryItemByContract(contractHash, "balances", dictItem);

    File.WriteAllText("res_ReadBalance.json", response.Result.GetRawText());

    var result = response.Parse();
    var balance = result.StoredValue.CLValue.ToBigInteger();
    Console.WriteLine("Balance: " + balance.ToString() + " $CSSDK");
}
catch (RpcClientException e)
{
    if (e.RpcError.Code == -32003)
        Console.WriteLine("Allowance not found!");
    else
        throw;
}
```
