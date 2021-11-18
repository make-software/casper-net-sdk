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
            KeyPair fromKey,
            PublicKey toKey,
            BigInteger amount,
            BigInteger paymentAmount,
            string chainName,
            ulong? idTransfer = null,
            ulong gasPrice = 1,
            ulong ttl = 1800000, //30m
            string sourcePurse = null
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey.PublicKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            var session = new TransferDeployItem(
                amount,
                toKey,
                sourcePurse==null ? null : CLValue.URef(sourcePurse),
                idTransfer);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }

        public static Deploy ContractDeploy(
            byte[] wasmBytes,
            KeyPair fromKey,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey.PublicKey,
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
            KeyPair fromKey,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey.PublicKey,
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
        
        public static Deploy ContractCallWithHash(
            string contractHash,
            string sessionEntryPoint,
            List<NamedArg> args,
            KeyPair fromKey,
            BigInteger paymentAmount,
            string chainName,
            ulong gasPrice = 1,
            ulong ttl = 1800000 //30m
        )
        {
            var header = new DeployHeader()
            {
                Account = fromKey.PublicKey,
                Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
                Ttl = ttl,
                ChainName = chainName,
                GasPrice = gasPrice
            };
            var payment = new ModuleBytesDeployItem(paymentAmount);
            var session = new StoredContractByHashDeployItem(contractHash, sessionEntryPoint, args);
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }
        
        public static Deploy DelegateTokens(
            byte[] delegateContractWasmBytes,
            KeyPair fromKey,
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
                Account = fromKey.PublicKey,
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
                CLValue.PublicKey(fromKey.PublicKey)));
            
            var deploy = new Deploy(header, payment, session);
            return deploy;
        }

        public static Deploy UndelegateTokens(
            byte[] undelegateContractWasmBytes,
            KeyPair fromKey,
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