using System;
using System.Collections.Generic;
using System.Numerics;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    public partial class Transaction
    {
        public abstract class TransactionV1Builder<T> where T : TransactionV1Builder<T> 
        {
            protected InitiatorAddr _from = null;
            protected string _chainName = null;
            protected DateTime? _timestamp = null;
            protected ulong _ttl = 1800000; //30m
            protected byte _gasPriceTolerance = 1;
            
            protected ITransactionV1Target _invocationTarget;
            protected ITransactionV1EntryPoint _entryPoint;
            protected TransactionCategory _category;
            protected ITransactionV1Scheduling _scheduling = TransactionScheduling.Standard;
            protected List<NamedArg> _runtimeArgs = new();
            
            public T From(PublicKey publicKey)
            {
                _from = Types.InitiatorAddr.FromPublicKey(publicKey);
                return (T)this;
            }
            
            public T From(AccountHashKey accountHashKey)
            {
                _from = Types.InitiatorAddr.FromAccountHash(accountHashKey);
                return (T)this;
            }
            
            public T ChainName(string chainName)
            {
                _chainName = chainName;
                return (T)this;
            }
            
            public T Timestamp(DateTime timestamp)
            {
                _timestamp = timestamp;
                return (T)this;
            }
            
            public T TTL(ulong ttl)
            {
                _ttl = ttl;
                return (T)this;
            }
            
            public T GasPriceTolerance(byte gasPriceTolerance)
            {
                _gasPriceTolerance = gasPriceTolerance;
                return (T)this;
            }
            
            public virtual TransactionV1 Build()
            {
                var body = new TransactionV1Body()
                {
                    RuntimeArgs = _runtimeArgs,
                    Target = _invocationTarget,
                    EntryPoint = _entryPoint,
                    Scheduling = _scheduling,
                    Category = _category,
                };
                var header = new TransactionV1Header()
                {
                    InitiatorAddr = _from,
                    Timestamp = DateUtils.ToEpochTime(_timestamp.HasValue ? _timestamp.Value : DateTime.UtcNow),
                    Ttl = _ttl,
                    ChainName = _chainName,
                    PricingMode = Types.PricingMode.Fixed(_gasPriceTolerance),
                };
                var transaction = new TransactionV1(header, body);
                return transaction;
            }
        }
        
        public class NativeTransferBuilder : TransactionV1Builder<NativeTransferBuilder>
        {
            //specific tx properties
            private CLValue _target = null;
            private CLValue _amount = CLValue.U512((BigInteger)0);
            private ulong? _idTransfer = null;

            public NativeTransferBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.Transfer;
                _category = TransactionCategory.Mint;
            }
            
            public NativeTransferBuilder Target(PublicKey publicKey)
            {
                _target = CLValue.PublicKey(publicKey);
                return this;
            }
            
            public NativeTransferBuilder Target(AccountHashKey accountHashKey)
            {
                _target = CLValue.ByteArray(accountHashKey.RawBytes);
                return this;
            }
            
            public NativeTransferBuilder Amount(ulong amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }
            
            public NativeTransferBuilder Amount(BigInteger amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }
            
            public NativeTransferBuilder Id(ulong id)
            {
                _idTransfer = id;
                return this;
            }
            
            public override TransactionV1 Build()
            {
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("target", _target));
                _runtimeArgs.Add(new NamedArg("amount", _amount));
                if (_idTransfer.HasValue)
                {
                    _runtimeArgs.Add(new NamedArg("id", CLValue.Option(CLValue.U64(_idTransfer.Value))));
                }
                
                return base.Build();
            }
        }
        
        public class NativeDelegateBuilder : TransactionV1Builder<NativeDelegateBuilder>
        {
            //specific tx properties
            private CLValue _validator = null;
            private CLValue _amount = CLValue.U512((BigInteger)0);
            
            public NativeDelegateBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.Delegate;
                _category = TransactionCategory.Auction;
            }
            
            public NativeDelegateBuilder Validator(PublicKey publicKey)
            {
                _validator = CLValue.PublicKey(publicKey);
                return this;
            }
            
            public NativeDelegateBuilder Amount(ulong amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }
            
            public NativeDelegateBuilder Amount(BigInteger amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }
            
            public override TransactionV1 Build()
            {
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("delegator", CLValue.PublicKey(_from.PublicKey)));
                _runtimeArgs.Add(new NamedArg("validator", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));
                
                return base.Build();
            }
        }
        
        public class NativeUndelegateBuilder : TransactionV1Builder<NativeUndelegateBuilder>
        {
            //specific tx properties
            private CLValue _validator = null;
            private CLValue _amount = CLValue.U512((BigInteger)0);
            
            public NativeUndelegateBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.Undelegate;
                _category = TransactionCategory.Auction;
            }
            
            public NativeUndelegateBuilder Validator(PublicKey publicKey)
            {
                _validator = CLValue.PublicKey(publicKey);
                return this;
            }
            
            public NativeUndelegateBuilder Amount(ulong amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }
            
            public NativeUndelegateBuilder Amount(BigInteger amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }
            
            public override TransactionV1 Build()
            {
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("delegator", CLValue.PublicKey(_from.PublicKey)));
                _runtimeArgs.Add(new NamedArg("validator", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));
                
                return base.Build();
            }
        }
        
        public class ContractCallBuilder : TransactionV1Builder<ContractCallBuilder>
        {
            public ContractCallBuilder()
            {
                _category = TransactionCategory.Small;
            }
            
            public ContractCallBuilder ByHash(string contractHash)
            {
                _invocationTarget = TransactionV1Target.StoredByHash(contractHash);
                return this;
            }
            
            public ContractCallBuilder ByName(string name)
            {
                _invocationTarget = TransactionV1Target.StoredByName(name);
                return this;
            }
            
            public ContractCallBuilder ByPackageHash(string contractHash)
            {
                _invocationTarget = TransactionV1Target.StoredByPackageHash(contractHash);
                return this;
            }
            
            public ContractCallBuilder ByPackageName(string name)
            {
                _invocationTarget = TransactionV1Target.StoredByPackageName(name);
                return this;
            }
            
            public ContractCallBuilder EntryPoint(string name)
            {
                _entryPoint = TransactionV1EntryPoint.Custom(name);
                return this;
            }
            
            public ContractCallBuilder InstallCategory()
            {
                _category = TransactionCategory.Large;
                return this;
            }
            
            public ContractCallBuilder LargeCategory()
            {
                _category = TransactionCategory.Large;
                return this;
            }
            
            public ContractCallBuilder MediumCategory()
            {
                _category = TransactionCategory.Medium;
                return this;
            }
            
            public ContractCallBuilder SmallCategory()
            {
                _category = TransactionCategory.Small;
                return this;
            }
            
            public ContractCallBuilder RuntimeArgs(List<NamedArg> args)
            {
                _runtimeArgs = args;
                return this;
            }
        }
        
        public class SessionBuilder : TransactionV1Builder<SessionBuilder>
        {
            private byte[] _wasm = null;
                
            public SessionBuilder()
            {
                _category = TransactionCategory.InstallUpgrade;
                _entryPoint = TransactionV1EntryPoint.Call;
            }
            
            public SessionBuilder Wasm(byte[] wasmBytes)
            {
                _invocationTarget = TransactionV1Target.Session(wasmBytes);
                return this;
            }
            
            public SessionBuilder InstallCategory()
            {
                _category = TransactionCategory.Large;
                return this;
            }
            
            public SessionBuilder LargeCategory()
            {
                _category = TransactionCategory.Large;
                return this;
            }
            
            public SessionBuilder MediumCategory()
            {
                _category = TransactionCategory.Medium;
                return this;
            }
            
            public SessionBuilder SmallCategory()
            {
                _category = TransactionCategory.Small;
                return this;
            }
            
            public SessionBuilder RuntimeArgs(List<NamedArg> args)
            {
                _runtimeArgs = args;
                return this;
            }
        }
    }
}
