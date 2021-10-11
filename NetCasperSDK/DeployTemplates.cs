using System;
using System.Collections.Generic;
using NetCasperSDK.Types;
using System.Numerics;
using NetCasperSDK.Utils;

namespace NetCasperSDK
{
    public class DeployTemplates
    {
        public static Deploy StandardTransfer(
            KeyPair fromKey,
            PublicKey toKey,
            BigInteger amount,
            BigInteger paymentAmount,
            string chainName,
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
                1234);
            
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
    }
}