using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    using ITransactionScheduling = ITransactionV1Scheduling;
    
    internal class TransactionCompat
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        [JsonPropertyName("Deploy")] 
        public Deploy Deploy { get; init; }
    
        /// <summary>
        /// A version 1 transaction.
        /// </summary>
        [JsonPropertyName("Version1")] 
        public TransactionV1 Version1 { get; init; }
    }
    
    /// <summary>
    /// A versioned wrapper for a TransactionV1 or Deploy.
    /// </summary>
    public partial class Transaction
    {
        protected TransactionVersion _version;

        /// <summary>
        /// Returns the version of the transaction (0=Deploy, 1=TransactionV1)  .
        /// </summary>
        public TransactionVersion Version
        {
            get { return _version; }
        }

        private Deploy _deploy;

        private TransactionV1 _transactionV1;

        public string Hash { get; init; }

        /// <summary>
        /// The address of the initiator of a transaction.
        /// </summary>
        public InitiatorAddr InitiatorAddr { get; set; }

        /// <summary>
        /// Timestamp formatted as per RFC 3339 
        /// </summary>
        public ulong Timestamp { get; set; }

        /// <summary>
        /// Duration of the Deploy in milliseconds (from timestamp).
        /// </summary>
        public ulong Ttl { get; set; }

        /// <summary>
        /// Name of the chain where the deploy is executed.
        /// </summary>
        public string ChainName { get; set; }

        /// <summary>
        /// List of signers and signatures for this transaction.
        /// </summary>
        public List<Approval> Approvals { get; init; }

        /// <summary>
        /// Pricing mode of a Transaction.
        /// </summary>
        public IPricingMode PricingMode { get; set; }
        
        public ITransactionScheduling Scheduling { get; set; }
        
        public ITransactionInvocation Invocation { get; init; }
        
        public interface ITransactionInvocation
        {
            List<NamedArg> RuntimeArgs { get; init; }

            CLValue GetRuntimeArgValue(string name);
        }
        
        public abstract class TransactionInvocation : ITransactionInvocation
        {
            public List<NamedArg> RuntimeArgs { get; init; }
            
            public CLValue GetRuntimeArgValue(string name)
            {
                var runtimeArg = RuntimeArgs.FirstOrDefault(a => a.Name.Equals(name));
                return runtimeArg?.Value;
            }
        }
        
        public class NativeTransactionInvocation : TransactionInvocation
        {
            public NativeEntryPoint Type { get; init; }
            
            public string Name { get { return Type.ToString(); } }
        }
        
        public class StoredTransactionInvocation : TransactionInvocation
        {
            public IInvocationTarget InvocationTarget { get; init; }
            
            public string EntryPoint { get; init; }
        }
        
        public class SessionTransactionInvocation : TransactionInvocation
        {
            public byte[] Wasm { get; init; }
        }

        /// <summary>
        /// Signs the transaction with a private key and adds a new Approval to it.
        /// </summary>
        public void Sign(KeyPair keyPair)
        {
            if(_deploy is not null)
                _deploy.Sign(keyPair);
            
            if(_transactionV1 is not null)
                _transactionV1.Sign(keyPair);
        }

        public class TransactionConverter : JsonConverter<Transaction>
        {
            public override Transaction Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    var compat = JsonSerializer.Deserialize<TransactionCompat>(ref reader, options);

                    if (compat != null && compat.Version1 != null)
                    {
                        return (Transaction)compat.Version1;
                    }

                    if (compat != null && compat.Deploy != null)
                    {
                        return (Transaction)compat.Deploy;
                    }

                    throw new JsonException("Cannot deserialize Transaction. Deploy or TransactionV1 not found");
                }
                catch (Exception e)
                {
                    throw new JsonException("Cannot deserialize Transaction. " + e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                Transaction value,
                JsonSerializerOptions options)
            {
                if (value._deploy is null && value._transactionV1 is null)
                    throw new JsonException("Cannot serialize empty Transaction. Deploy or TransactionV1 not found");

                var compat = new TransactionCompat()
                {
                    Deploy = value._deploy,
                    Version1 = value._transactionV1,
                };

                JsonSerializer.Serialize(writer, compat, options);
            }
        }

        public static explicit operator Transaction(Deploy deploy)
        {
            ITransactionInvocation invocation;
            if (deploy.Session is TransferDeployItem transferDeployItem)
            {
                invocation = new NativeTransactionInvocation()
                {
                    Type = NativeEntryPoint.Transfer,
                    RuntimeArgs = transferDeployItem.RuntimeArgs,
                };
            }
            else if (deploy.Session is ModuleBytesDeployItem moduleBytesDeployItem)
            {
                invocation = new SessionTransactionInvocation()
                {
                    Wasm = moduleBytesDeployItem.ModuleBytes,
                    RuntimeArgs = moduleBytesDeployItem.RuntimeArgs,
                };
            }
            else if (deploy.Session is StoredContractByHashDeployItem storedContractByHash)
            {
                invocation = new StoredTransactionInvocation()
                {
                    InvocationTarget = new ByHashInvocationTarget { Hash = storedContractByHash.Hash },
                    EntryPoint = storedContractByHash.EntryPoint,
                    RuntimeArgs = storedContractByHash.RuntimeArgs,
                };
            }
            else if (deploy.Session is StoredContractByNameDeployItem storedContractByName)
            {
                invocation = new StoredTransactionInvocation()
                {
                    InvocationTarget = new ByNameInvocationTarget { Name = storedContractByName.Name },
                    EntryPoint = storedContractByName.EntryPoint,
                    RuntimeArgs = storedContractByName.RuntimeArgs,
                };
            }
            else if (deploy.Session is StoredVersionedContractByHashDeployItem storedVersionedContractByHash)
            {
                invocation = new StoredTransactionInvocation()
                {
                    InvocationTarget = new ByPackageHashInvocationTarget
                    {
                        Hash = storedVersionedContractByHash.Hash, 
                        Version = storedVersionedContractByHash.Version
                    },
                    EntryPoint = storedVersionedContractByHash.EntryPoint,
                    RuntimeArgs = storedVersionedContractByHash.RuntimeArgs,
                };
            }
            else if (deploy.Session is StoredVersionedContractByNameDeployItem storedVersionedContractByName)
            {
                invocation = new StoredTransactionInvocation()
                {
                    InvocationTarget = new ByPackageNameInvocationTarget
                    {
                        Name = storedVersionedContractByName.Name, 
                        Version = storedVersionedContractByName.Version
                    },
                    EntryPoint = storedVersionedContractByName.EntryPoint,
                    RuntimeArgs = storedVersionedContractByName.RuntimeArgs,
                };
            }
            else
            {
                throw new ArgumentException("No valid session object in the deploy to convert to Transaction");
            }

            IPricingMode pricingMode;
            if (deploy.Payment is ModuleBytesDeployItem paymentModule)
            {
                var amountArg = paymentModule.RuntimeArgs.FirstOrDefault(arg => arg.Name == "amount");
                if (amountArg == null)
                    pricingMode = Types.PricingMode.PaymentLimited(0, (byte)deploy.Header.GasPrice, false);
                else
                {
                    var paymentAmount = (ulong)amountArg.Value.ToBigInteger();
                    pricingMode = Types.PricingMode.PaymentLimited(paymentAmount, (byte)deploy.Header.GasPrice, paymentModule.ModuleBytes == null);
                }
            }
            else
            {
                throw new ArgumentException("No valid payment object in the deploy to convert to Transaction");
            }

            return new Transaction()
            {
                _deploy = deploy,
                _version = TransactionVersion.Deploy,
                Hash = deploy.Hash,
                InitiatorAddr = InitiatorAddr.FromPublicKey(deploy.Header.Account),
                Timestamp = deploy.Header.Timestamp,
                Ttl = deploy.Header.Ttl,
                ChainName = deploy.Header.ChainName,
                PricingMode = pricingMode,
                Approvals = deploy.Approvals,
                Scheduling = TransactionScheduling.Standard,
                Invocation = invocation,
            };
        }
        
        public static explicit operator Transaction(TransactionV1 transactionV1)
        {
            ITransactionInvocation transactionInvocation;

            if(transactionV1.Payload.Target is NativeTransactionV1Target &&
               transactionV1.Payload.EntryPoint is NativeTransactionV1EntryPoint nativeEntryPoint)
            {
                transactionInvocation = new NativeTransactionInvocation()
                {
                    Type = nativeEntryPoint.Type,
                    RuntimeArgs = transactionV1.Payload.RuntimeArgs,
                };
            }
            else if (transactionV1.Payload.Target is StoredTransactionV1Target storedTarget &&
                     transactionV1.Payload.EntryPoint is CustomTransactionV1EntryPoint customEntryPoint)
            {
                transactionInvocation = new StoredTransactionInvocation()
                {
                    InvocationTarget = storedTarget.Id,
                    EntryPoint = customEntryPoint.Name,
                    RuntimeArgs = transactionV1.Payload.RuntimeArgs,
                };
            }
            else if (transactionV1.Payload.Target is SessionTransactionV1Target sessionTarget &&
                     transactionV1.Payload.EntryPoint.Name.Equals("Call", StringComparison.InvariantCultureIgnoreCase))
            {
                transactionInvocation = new SessionTransactionInvocation()
                {
                    Wasm = sessionTarget.ModuleBytes,
                    RuntimeArgs = transactionV1.Payload.RuntimeArgs,
                };
            }
            else
            {
                throw new Exception("Invalid Target or EntryPoint data, or not compatible.");
            }

            return new Transaction()
            {
                _transactionV1 = transactionV1,
                _version = TransactionVersion.TransactionV1,
                Hash = transactionV1.Hash,
                InitiatorAddr = transactionV1.Payload.InitiatorAddr,
                Timestamp = transactionV1.Payload.Timestamp,
                Ttl = transactionV1.Payload.Ttl,
                ChainName = transactionV1.Payload.ChainName,
                PricingMode = transactionV1.Payload.PricingMode,
                Approvals = transactionV1.Approvals,
                Scheduling = transactionV1.Payload.Scheduling,
                Invocation = transactionInvocation,
            };
        }
        
        public static explicit operator Deploy(Transaction transaction)
        {
            if (transaction._version == TransactionVersion.Deploy)
                return transaction._deploy;

            throw new InvalidCastException("TransactionV1 transaction cannot be converted to Deploy");
        }
        
        public static explicit operator TransactionV1(Transaction transaction)
        {
            if (transaction._version == TransactionVersion.TransactionV1)
                return transaction._transactionV1;

            throw new InvalidCastException("Deploy transaction cannot be converted to TransactionV1");
        }
    }
}