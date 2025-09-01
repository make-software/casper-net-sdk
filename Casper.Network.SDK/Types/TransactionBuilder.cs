using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Casper.Network.SDK.Utils;

namespace Casper.Network.SDK.Types
{
    public partial class Transaction
    {
        [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
        public class RequiredArgAttribute : Attribute
        {
        }
        
        public abstract class TransactionV1Builder<T> where T : TransactionV1Builder<T>
        {
            [RequiredArg]
            protected InitiatorAddr _initiatorAddr;
            [RequiredArg]
            protected string _chainName;
            protected DateTime? _timestamp;
            protected ulong _ttl = 1800000; //30m
            [RequiredArg]
            protected IPricingMode _pricingMode;
            [RequiredArg]
            protected ITransactionV1Target _invocationTarget;
            [RequiredArg]
            protected ITransactionV1EntryPoint _entryPoint;
            protected ITransactionV1Scheduling _scheduling = TransactionScheduling.Standard;
            [RequiredArg]
            protected List<NamedArg> _runtimeArgs = new();

            public T From(PublicKey publicKey)
            {
                _initiatorAddr = Types.InitiatorAddr.FromPublicKey(publicKey);
                return (T)this;
            }

            public T From(AccountHashKey accountHashKey)
            {
                _initiatorAddr = Types.InitiatorAddr.FromAccountHash(accountHashKey);
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

            public T Payment(IPricingMode pricingMode)
            {
                _pricingMode = pricingMode;
                return (T)this;
            }
            
            public T Payment(ulong amount, byte gasPriceTolerance = 1)
            {
                _pricingMode = Types.PricingMode.PaymentLimited(amount, gasPriceTolerance);
                return (T)this;
            }
            
            protected void ValidateRequiredProperties()
            {
                var missingProperties = this.GetType()
                    .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Where(field => Attribute.IsDefined(field, typeof(RequiredArgAttribute)) && field.GetValue(this) == null)
                    .Select(field => field.Name)
                    .ToList();

                if (missingProperties.Any())
                {
                    throw new InvalidOperationException($"The following required properties are missing: {string.Join(", ", missingProperties)}");
                }
            }

            public virtual Transaction Build()
            {
                var payload = new TransactionV1Payload()
                {
                    InitiatorAddr = _initiatorAddr,
                    Timestamp = DateUtils.ToEpochTime(_timestamp.HasValue ? _timestamp.Value : DateTime.UtcNow),
                    Ttl = _ttl,
                    ChainName = _chainName,
                    PricingMode = _pricingMode,
                    RuntimeArgs = _runtimeArgs,
                    Target = _invocationTarget,
                    EntryPoint = _entryPoint,
                    Scheduling = _scheduling,
                };
                var transactionV1 = new TransactionV1(payload);
                return (Transaction)transactionV1;
            }
        }

        public class NativeTransferBuilder : TransactionV1Builder<NativeTransferBuilder>
        {
            //specific tx properties
            private URef _source;
            [RequiredArg]
            private CLValue _target;
            [RequiredArg]
            private CLValue _amount = CLValue.U512((BigInteger)0);
            private ulong? _idTransfer;

            public NativeTransferBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.Transfer;
            }

            public NativeTransferBuilder Source(URef uRef)
            {
                _source = uRef;
                return this;
            }

            public NativeTransferBuilder Target(IPurseIdentifier purseIdentifier)
            {
                if (purseIdentifier is URef uref)
                    _target = CLValue.URef(uref);
                else if (purseIdentifier is AccountHashKey accountHashKey)
                    _target = CLValue.ByteArray(accountHashKey.RawBytes);
                else if (purseIdentifier is PublicKey publicKey)
                    _target = CLValue.PublicKey(publicKey);
                else if (purseIdentifier is AddressableEntityKey addressableEntityKey)
                    _target = CLValue.ByteArray(addressableEntityKey.RawBytes);
                else
                    throw new Exception("Invalid purse identifier");
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

            public override Transaction Build()
            {
                ValidateRequiredProperties();

                _runtimeArgs = new List<NamedArg>();
                if(_source is not null)
                    _runtimeArgs.Add(new NamedArg("source", _source));
                _runtimeArgs.Add(new NamedArg("target", _target));
                _runtimeArgs.Add(new NamedArg("amount", _amount));
                if (_idTransfer.HasValue)
                {
                    _runtimeArgs.Add(new NamedArg("id", CLValue.Option(CLValue.U64(_idTransfer.Value))));
                }

                return base.Build();
            }
        }
        
        public class NativeAddBidBuilder : TransactionV1Builder<NativeAddBidBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private CLValue _validator;
            [RequiredArg]
            private CLValue _amount;
            [RequiredArg]
            private CLValue _delegationRate;
            private CLValue _minimumDelegationAmount;
            private CLValue _maximumDelegationAmount;
            private CLValue _reservedSlots;
            
            public NativeAddBidBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.AddBid;
            }

            public NativeAddBidBuilder Validator(PublicKey publicKey)
            {
                _validator = CLValue.PublicKey(publicKey);
                return this;
            }

            public NativeAddBidBuilder Amount(ulong amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }

            public NativeAddBidBuilder Amount(BigInteger amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }

            public NativeAddBidBuilder DelegationRate(byte delegationRate)
            {
                _delegationRate = CLValue.U8(delegationRate);
                return this;
            }

            public NativeAddBidBuilder MinimumDelegationAmount(ulong minimumDelegationAmount)
            {
                _minimumDelegationAmount = CLValue.U64(minimumDelegationAmount);
                return this;
            }

            public NativeAddBidBuilder MaximumDelegationAmount(ulong maximumDelegationAmount)
            {
                _maximumDelegationAmount = CLValue.U64(maximumDelegationAmount);
                return this;
            }

            public NativeAddBidBuilder ReservedSlots(uint reservedSlots)
            {
                _reservedSlots = CLValue.U32(reservedSlots);
                return this;
            }

            public override Transaction Build()
            {
                ValidateRequiredProperties();
                
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("public_key", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));
                _runtimeArgs.Add(new NamedArg("delegation_rate", _delegationRate));
                if(_minimumDelegationAmount != null)
                    _runtimeArgs.Add(new NamedArg("minimum_delegation_amount", _minimumDelegationAmount));
                if(_maximumDelegationAmount != null)
                _runtimeArgs.Add(new NamedArg("maximum_delegation_amount", _maximumDelegationAmount));
                if(_reservedSlots != null) 
                    _runtimeArgs.Add(new NamedArg("reserved_slots", _reservedSlots));
                return base.Build();
            }
        }

        public class NativeWithdrawBidBuilder : TransactionV1Builder<NativeWithdrawBidBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private CLValue _validator;
            [RequiredArg]
            private CLValue _amount = CLValue.U512((BigInteger)0);

            public NativeWithdrawBidBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.WithdrawBid;
            }

            public NativeWithdrawBidBuilder Validator(PublicKey publicKey)
            {
                _validator = CLValue.PublicKey(publicKey);
                return this;
            }

            public NativeWithdrawBidBuilder Amount(ulong amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }

            public NativeWithdrawBidBuilder Amount(BigInteger amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }

            public override Transaction Build()
            {
                ValidateRequiredProperties();

                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("public_key", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));
                return base.Build();
            }
        }

        public class NativeDelegateBuilder : TransactionV1Builder<NativeDelegateBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private CLValue _validator;
            [RequiredArg]
            private CLValue _amount = CLValue.U512((BigInteger)0);

            public NativeDelegateBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.Delegate;
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

            public override Transaction Build()
            {
                ValidateRequiredProperties();

                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("delegator", CLValue.PublicKey(_initiatorAddr.PublicKey)));
                _runtimeArgs.Add(new NamedArg("validator", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));

                return base.Build();
            }
        }

        public class NativeUndelegateBuilder : TransactionV1Builder<NativeUndelegateBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private CLValue _validator;
            [RequiredArg]
            private CLValue _amount = CLValue.U512((BigInteger)0);

            public NativeUndelegateBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.Undelegate;
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

            public override Transaction Build()
            {
                ValidateRequiredProperties();

                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("delegator", CLValue.PublicKey(_initiatorAddr.PublicKey)));
                _runtimeArgs.Add(new NamedArg("validator", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));

                return base.Build();
            }
        }

        public class NativeRedelegateBuilder : TransactionV1Builder<NativeRedelegateBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private CLValue _validator;
            [RequiredArg]
            private CLValue _newValidator;
            [RequiredArg]
            private CLValue _amount = CLValue.U512((BigInteger)0);

            public NativeRedelegateBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.Redelegate;
            }

            public NativeRedelegateBuilder Validator(PublicKey publicKey)
            {
                _validator = CLValue.PublicKey(publicKey);
                return this;
            }

            public NativeRedelegateBuilder NewValidator(PublicKey publicKey)
            {
                _newValidator = CLValue.PublicKey(publicKey);
                return this;
            }

            public NativeRedelegateBuilder Amount(ulong amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }

            public NativeRedelegateBuilder Amount(BigInteger amount)
            {
                _amount = CLValue.U512(amount);
                return this;
            }

            public override Transaction Build()
            {
                ValidateRequiredProperties();

                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("delegator", CLValue.PublicKey(_initiatorAddr.PublicKey)));
                _runtimeArgs.Add(new NamedArg("validator", _validator));
                _runtimeArgs.Add(new NamedArg("new_validator", _newValidator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));

                return base.Build();
            }
        }

        public class NativeActivateBidBuilder : TransactionV1Builder<NativeActivateBidBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private CLValue _validator;

            public NativeActivateBidBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.ActivateBid;
            }

            public NativeActivateBidBuilder Validator(PublicKey publicKey)
            {
                _validator = CLValue.PublicKey(publicKey);
                return this;
            }

            public override Transaction Build()
            {
                ValidateRequiredProperties();
            
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("validator", _validator));
                return base.Build();
            }
        }
        
        public class NativeChangeBidPublicKeyBuilder : TransactionV1Builder<NativeChangeBidPublicKeyBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private CLValue _public_key;
            [RequiredArg]
            private CLValue _new_public_key;

            public NativeChangeBidPublicKeyBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.ChangeBidPublicKey;
            }

            public NativeChangeBidPublicKeyBuilder PublicKey(PublicKey publicKey)
            {
                _public_key = CLValue.PublicKey(publicKey);
                return this;
            }

            public NativeChangeBidPublicKeyBuilder NewPublicKey(PublicKey publicKey)
            {
                _new_public_key = CLValue.PublicKey(publicKey);
                return this;
            }

            public override Transaction Build()
            {
                ValidateRequiredProperties();

                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("public_key", _public_key));
                _runtimeArgs.Add(new NamedArg("new_public_key", _new_public_key));
                return base.Build();
            }
        }

        public class NativeAddReservationsBuilder : TransactionV1Builder<NativeAddReservationsBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private List<Reservation> _reservations;

            public NativeAddReservationsBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.AddReservations;
            }

            public NativeAddReservationsBuilder Reservations(List<Reservation> reservations)
            {
                _reservations = reservations;
                return this;
            }

            public override Transaction Build()
            {
                ValidateRequiredProperties();

                var list = _reservations.Select(r => r.ToCLValue());
                _runtimeArgs.Add(new NamedArg("reservations", CLValue.List(list.ToArray())));
                return base.Build();
            }
        }
        
        public class NativeCancelReservationsBuilder : TransactionV1Builder<NativeCancelReservationsBuilder>
        {
            //specific tx properties
            [RequiredArg]
            private PublicKey _validator;
            [RequiredArg]
            private List<DelegatorKind> _delegators;

            public NativeCancelReservationsBuilder()
            {
                _invocationTarget = TransactionV1Target.Native;
                _entryPoint = TransactionV1EntryPoint.CancelReservations;
            }

            public NativeCancelReservationsBuilder Validator(PublicKey validator)
            {
                _validator = validator;
                return this;
            }
            
            public NativeCancelReservationsBuilder Delegators(List<DelegatorKind> delegators)
            {
                _delegators = delegators;
                return this;
            }

            public override Transaction Build()
            {
                ValidateRequiredProperties();

                _runtimeArgs.Add(new NamedArg("validator", CLValue.PublicKey(_validator)));
                var list = _delegators.Select(r => r.ToCLValue());
                _runtimeArgs.Add(new NamedArg("delegators", CLValue.List(list.ToArray())));
                return base.Build();
            }
        }

        public class ContractCallBuilder : TransactionV1Builder<ContractCallBuilder>
        {
            public ContractCallBuilder()
            {
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

            public ContractCallBuilder ByPackageHash(string contractPackageHash, UInt32? version = null, UInt32? protocolVersionMajor = null)
            {
                _invocationTarget = TransactionV1Target.StoredByPackageHash(contractPackageHash, version, protocolVersionMajor);
                return this;
            }

            public ContractCallBuilder ByPackageName(string packageName, UInt32? version = null, UInt32? protocolVersionMajor = null)
            {
                _invocationTarget = TransactionV1Target.StoredByPackageName(packageName, version, protocolVersionMajor);
                return this;
            }

            public ContractCallBuilder EntryPoint(string name)
            {
                _entryPoint = TransactionV1EntryPoint.Custom(name);
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
            private bool _isInstallOrUpgrade = false;

            public SessionBuilder()
            {
                _entryPoint = TransactionV1EntryPoint.Call;
            }

            public SessionBuilder Wasm(byte[] wasmBytes)
            {
                var target = TransactionV1Target.Session(wasmBytes);
                target.IsInstallUpgrade = _isInstallOrUpgrade;
                _invocationTarget = target;
                return this;
            }

            public SessionBuilder InstallOrUpgrade()
            {
                _isInstallOrUpgrade = true;
                if (_invocationTarget is SessionTransactionV1Target sessionTarget)
                    sessionTarget.IsInstallUpgrade = true;
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