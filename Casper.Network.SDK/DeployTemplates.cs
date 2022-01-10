using System;
using System.Collections.Generic;
using Casper.Network.SDK.Types;
using System.Numerics;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK
{
    /// <summary>
    /// Create a Deploy based on one of the existing templates for the most common
    /// actions: transfers, contract deployment, contract call, delegation and undelegation.
    /// </summary>
    public class DeployTemplates
    {
        /// <summary>
        /// Creates a Deploy to make a Transfer of $CSPR between two accounts.
        /// Sign the deploy with 'fromKey' private key before sending it to the network.
        /// </summary>
        /// <param name="fromKey">Source account public key.</param>
        /// <param name="toKey">Target account public key.</param>
        /// <param name="amount">Amount of $CSPR (in motes) to transfer.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the transfer.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="idTransfer">Id number of the transfer. Or null if no id is needed.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to make a transfer of $CSPR between two accounts.</returns>
        public static Deploy StandardTransfer(
            PublicKey fromKey,
            PublicKey toKey,
            BigInteger amount,
            BigInteger paymentAmount,
            string chainName,
            ulong? idTransfer = null,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            var session = new TransferDeployItem(
                amount,
                new AccountHashKey(toKey),
                idTransfer);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }

        /// <summary>
        /// Creates a Deploy object to deploy a contract in the network. The code of the contract
        /// must be compiled previously in wasm bytecode.
        /// </summary>
        /// <param name="wasmBytes">Array of bytes containing the compiled contract (in wasm).</param>
        /// <param name="fromKey">Public key of the account that deploys the contract.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the contract deployment.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to deploy a contract in the network.</returns>
        public static Deploy ContractDeploy(
            byte[] wasmBytes,
            PublicKey fromKey,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            var session = new ModuleBytesDeployItem(wasmBytes);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        /// <summary>
        /// Creates a Deploy object to call an entry point in a contract. The contract is referenced
        /// by a named key in the caller's account.
        /// </summary>
        /// <param name="contractName">The named key in the caller account that contains a reference to the contract hash key.</param>
        /// <param name="contractEntryPoint">The entry point called in the contract.</param>
        /// <param name="args">List of the input runtime arguments.</param>
        /// <param name="fromKey">Public key of the caller account.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the contract call.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to call a contract in the network.</returns>
        public static Deploy ContractCall(
            string contractName,
            string contractEntryPoint,
            List<NamedArg> args,
            PublicKey fromKey,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            var session = new StoredContractByNameDeployItem(contractName,contractEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        /// <summary>
        /// Creates a Deploy object to call an entry point in a contract. The contract is referenced
        /// by the contract hash.
        /// </summary>
        /// <param name="contractHash">The contract hash key.</param>
        /// <param name="contractEntryPoint">The entry point called in the contract.</param>
        /// <param name="args">List of the input runtime arguments.</param>
        /// <param name="fromKey">Public key of the caller account.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the contract call.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to call a contract in the network.</returns>
        public static Deploy ContractCall(
            HashKey contractHash,
            string contractEntryPoint,
            List<NamedArg> args,
            PublicKey fromKey,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            var session = new StoredContractByHashDeployItem(contractHash.ToHexString(), contractEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        /// <summary>
        /// Creates a Deploy object to call an entry point in a versioned contract. The contract is
        /// referenced by a named key in the caller's account containing the contract package hash.
        /// </summary>
        /// <param name="contractName">The named key in the caller account that contains a reference to the contract package key.</param>
        /// <param name="version">Version number in the contract package to call.</param>
        /// <param name="contractEntryPoint">The entry point called in the contract.</param>
        /// <param name="args">List of the input runtime arguments.</param>
        /// <param name="fromKey">Public key of the caller account.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the contract call.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to call a versioned contract in the network.</returns>        
        public static Deploy VersionedContractCall(
            string contractName,
            uint? version,
            string contractEntryPoint,
            List<NamedArg> args,
            PublicKey fromKey,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            var session = new StoredVersionedContractByNameDeployItem(contractName, version, 
                contractEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        /// <summary>
        /// Creates a Deploy object to call an entry point in a versioned contract. The contract is
        /// referenced by the contract package hash.
        /// </summary>
        /// <param name="contractHash">The contract package hash key.</param>
        /// <param name="version">Version number in the contract package to call.</param>
        /// <param name="contractEntryPoint">The entry point called in the contract.</param>
        /// <param name="args">List of the input runtime arguments.</param>
        /// <param name="fromKey">Public key of the caller account.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the contract call.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to call a versioned contract in the network.</returns>
        public static Deploy VersionedContractCall(
            HashKey contractHash,
            uint? version,
            string contractEntryPoint,
            List<NamedArg> args,
            PublicKey fromKey,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            var session = new StoredVersionedContractByHashDeployItem(contractHash.ToHexString(), version, 
                contractEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        /// <summary>
        /// Creates a Deploy object to delegate tokens to a validator for staking.
        /// </summary>
        /// <param name="delegateContractWasmBytes">Array of bytes containing the delegation contract (compiled in wasm).</param>
        /// <param name="fromKey">Public key of the account that delegates the tokens.</param>
        /// <param name="validatorPK">Public key of the validator to which the tokens are delegated for staking.</param>
        /// <param name="amount">Amount of $CSPR (in motes) to delegate.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the delegation deployment.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to delegate tokens to a validator for staking.</returns>
        public static Deploy DelegateTokens(
            byte[] delegateContractWasmBytes,
            PublicKey fromKey,
            PublicKey validatorPK,
            BigInteger amount,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            
            var session = new ModuleBytesDeployItem(delegateContractWasmBytes);
            session.RuntimeArgs.Add(new NamedArg("validator", 
                CLValue.PublicKey(validatorPK)));
            session.RuntimeArgs.Add(new NamedArg("amount",
                CLValue.U512(amount)));
            session.RuntimeArgs.Add(new NamedArg("delegator",
                CLValue.PublicKey(fromKey)));
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }

        /// <summary>
        /// Creates a Deploy object to undelegate tokens from a validator.
        /// </summary>
        /// <param name="undelegateContractWasmBytes">Array of bytes containing the "undelegation" contract (compiled in wasm).</param>
        /// <param name="fromKey">Public key of the account that undelegates the tokens.</param>
        /// <param name="validatorPK">Public key of the validator from which the tokens are undelegated.</param>
        /// <param name="amount">Amount of $CSPR (in motes) to undelegate.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the delegation deployment.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to undelegate tokens from a validator.</returns>
        public static Deploy UndelegateTokens(
            byte[] undelegateContractWasmBytes,
            PublicKey fromKey,
            PublicKey validatorPK,
            BigInteger amount,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            return DelegateTokens(undelegateContractWasmBytes,
                fromKey,
                validatorPK,
                amount,
                paymentAmount,
                chainName,
                gasPrice,
                ttl);
        }


        private static Deploy CallAuctionContract(
            HashKey contractHash,
            string entryPoint,
            PublicKey fromKey,
            PublicKey validatorPK,
            BigInteger amount,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);

            var runtimeArgs = new List<NamedArg>
            {
                new NamedArg("validator", CLValue.PublicKey(validatorPK)),
                new NamedArg("amount", CLValue.U512(amount)),
                new NamedArg("delegator", CLValue.PublicKey(fromKey))
            };
            var session = new StoredContractByHashDeployItem(contractHash.ToHexString(),
                entryPoint, runtimeArgs);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        
        /// <summary>
        /// Creates a Deploy object to call the auction contract to delegate tokens to a validator for staking.
        /// Make sure you use the correct contract hash for the network you're sending the deploy!
        /// </summary>
        /// <param name="contractHash">Hash of the delegation contract in the network.</param>
        /// <param name="fromKey">Public key of the account that delegates the tokens.</param>
        /// <param name="validatorPK">Public key of the validator to which the tokens are delegated for staking.</param>
        /// <param name="amount">Amount of $CSPR (in motes) to delegate.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the delegation deployment.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to delegate tokens to a validator for staking.</returns>
        public static Deploy DelegateTokens(
            HashKey contractHash,
            PublicKey fromKey,
            PublicKey validatorPK,
            BigInteger amount,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            return CallAuctionContract(contractHash,
                "delegate",
                fromKey,
                validatorPK,
                amount,
                paymentAmount,
                chainName,
                gasPrice,
                ttl);
        }
        
        /// <summary>
        /// Creates a Deploy object to call the auction contract to delegate tokens to a validator for staking.
        /// Make sure you use the correct contract hash for the network you're sending the deploy!
        /// </summary>
        /// <param name="contractHash">Hash of the delegation contract in the network.</param>
        /// <param name="fromKey">Public key of the account that delegates the tokens.</param>
        /// <param name="validatorPK">Public key of the validator to which the tokens are delegated for staking.</param>
        /// <param name="amount">Amount of $CSPR (in motes) to delegate.</param>
        /// <param name="paymentAmount">Amount of $CSPR (in motes) to pay for the delegation deployment.</param>
        /// <param name="chainName">Name of the network that will execute the Deploy.</param>
        /// <param name="gasPrice">Gas price. Use 1 unless you need a different value.</param>
        /// <param name="ttl">Validity of the Deploy since the creation (in milliseconds).</param>
        /// <returns>A deploy configured to delegate tokens to a validator for staking.</returns>
        public static Deploy UndelegateTokens(
            HashKey contractHash,
            PublicKey fromKey,
            PublicKey validatorPK,
            BigInteger amount,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            return CallAuctionContract(contractHash,
                "undelegate",
                fromKey,
                validatorPK,
                amount,
                paymentAmount,
                chainName,
                gasPrice,
                ttl);
        }
    }
}