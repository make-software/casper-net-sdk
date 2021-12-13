using System;
using System.Collections.Generic;
using Casper.Network.SDK.Types;
using System.Numerics;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK
{
    public class DeployTemplates
    {
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
        
        public static Deploy ContractCall(
            string sessionName,
            string sessionEntryPoint,
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
            var session = new StoredContractByNameDeployItem(sessionName,sessionEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        public static Deploy ContractCall(
            HashKey contractHash,
            string sessionEntryPoint,
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
            var session = new StoredContractByHashDeployItem(contractHash.ToHexString(), sessionEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        public static Deploy VersionedContractCall(
            string sessionName,
            uint? version,
            string sessionEntryPoint,
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
            var session = new StoredVersionedContractByNameDeployItem(sessionName, version, 
                sessionEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        public static Deploy VersionedContractCall(
            HashKey contractHash,
            uint? version,
            string sessionEntryPoint,
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
                sessionEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
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
    }
}