using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

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

            protected IPricingMode _pricingMode;
            protected ITransactionV1Target _invocationTarget;
            protected ITransactionV1EntryPoint _entryPoint;
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

            public T Payment(IPricingMode pricingMode)
            {
                _pricingMode = pricingMode;
                return (T)this;
            }

            public virtual TransactionV1 Build()
            {
                var payload = new TransactionV1Payload()
                {
                    InitiatorAddr = _from,
                    Timestamp = DateUtils.ToEpochTime(_timestamp.HasValue ? _timestamp.Value : DateTime.UtcNow),
                    Ttl = _ttl,
                    ChainName = _chainName,
                    PricingMode = _pricingMode,
                    RuntimeArgs = _runtimeArgs,
                    Target = _invocationTarget,
                    EntryPoint = _entryPoint,
                    Scheduling = _scheduling,
                };
                return new TransactionV1(payload);
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

        public class NativeRedelegateBuilder : TransactionV1Builder<NativeRedelegateBuilder>
        {
            //specific tx properties
            private CLValue _validator = null;
            private CLValue _newValidator = null;
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

            public override TransactionV1 Build()
            {
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("delegator", CLValue.PublicKey(_from.PublicKey)));
                _runtimeArgs.Add(new NamedArg("validator", _validator));
                _runtimeArgs.Add(new NamedArg("new_validator", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));

                return base.Build();
            }
        }

        public class NativeActivateBidBuilder : TransactionV1Builder<NativeActivateBidBuilder>
        {
            //specific tx properties
            private CLValue _validator = null;

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

            public override TransactionV1 Build()
            {
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("validator", _validator));
                return base.Build();
            }
        }

        public class NativeChangeBidPublicKeyBuilder : TransactionV1Builder<NativeChangeBidPublicKeyBuilder>
        {
            //specific tx properties
            private CLValue _public_key = null;
            private CLValue _new_public_key = null;

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

            public override TransactionV1 Build()
            {
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("public_key", _public_key));
                _runtimeArgs.Add(new NamedArg("new_public_key", _new_public_key));
                return base.Build();
            }
        }

        public class NativeAddBidBuilder : TransactionV1Builder<NativeAddBidBuilder>
        {
            //specific tx properties
            private CLValue _validator = null;
            private CLValue _amount = CLValue.U512((BigInteger)0);
            private CLValue _delegationRate = CLValue.U8(100);
            private CLValue _minimumDelegationAmount = CLValue.OptionNone(CLType.U64);
            private CLValue _maximumDelegationAmount = CLValue.OptionNone(CLType.U64);
            private CLValue _reservedSlots = null;
            
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
                _minimumDelegationAmount = CLValue.Option(CLValue.U64(minimumDelegationAmount));
                _minimumDelegationAmount = CLValue.U64(minimumDelegationAmount);
                return this;
            }

            public NativeAddBidBuilder MaximumDelegationAmount(ulong maximumDelegationAmount)
            {
                _maximumDelegationAmount = CLValue.Option(CLValue.U64(maximumDelegationAmount));
                _maximumDelegationAmount = CLValue.U64(maximumDelegationAmount);
                return this;
            }

            public NativeAddBidBuilder ReservedSlots(uint reservedSlots)
            {
                _reservedSlots = CLValue.U32(reservedSlots);
                // _reservedSlots = CLValue.OptionNone(CLType.U32);
                return this;
            }

            public override TransactionV1 Build()
            {
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("public_key", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));
                _runtimeArgs.Add(new NamedArg("delegation_rate", _delegationRate));
                _runtimeArgs.Add(new NamedArg("minimum_delegation_amount", _minimumDelegationAmount));
                _runtimeArgs.Add(new NamedArg("maximum_delegation_amount", _maximumDelegationAmount));
                if(_reservedSlots != null) 
                    _runtimeArgs.Add(new NamedArg("reserved_slots", _reservedSlots));
                return base.Build();
            }
        }

        public class NativeWithdrawBidBuilder : TransactionV1Builder<NativeWithdrawBidBuilder>
        {
            //specific tx properties
            private CLValue _validator = null;
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

            public override TransactionV1 Build()
            {
                _runtimeArgs = new List<NamedArg>();
                _runtimeArgs.Add(new NamedArg("public_key", _validator));
                _runtimeArgs.Add(new NamedArg("amount", _amount));
                return base.Build();
            }
        }

        public class NativeAddReservationsBuilder : TransactionV1Builder<NativeAddReservationsBuilder>
        {
            //specific tx properties
            private List<Reservation> _reservations = null;

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

            public override TransactionV1 Build()
            {
                var list = _reservations.Select(r => r.ToCLValue());
                _runtimeArgs.Add(new NamedArg("reservations", CLValue.List(list.ToArray())));
                return base.Build();
            }
        }
        
        public class NativeCancelReservationsBuilder : TransactionV1Builder<NativeCancelReservationsBuilder>
        {
            //specific tx properties
            private PublicKey _validator = null;
            private List<DelegatorKind> _delegators = null;

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

            public override TransactionV1 Build()
            {
                _runtimeArgs.Add(new NamedArg("validator", CLValue.PublicKey(_validator)));
                // var list = _delegators.Select(r => r.ToCLValue());
                var list = _delegators.Select(r => CLValue.PublicKey(r.PublicKey));
                _runtimeArgs.Add(new NamedArg("delegators", CLValue.List(list.ToArray())));
                return base.Build();
            }
        }

        public class ContractCallBuilder : TransactionV1Builder<ContractCallBuilder>
        {
            private ulong _transferredValue = 0;

            public ContractCallBuilder()
            {
            }

            public ContractCallBuilder ByHash(string contractHash)
            {
                _invocationTarget = TransactionV1Target.StoredByHash(contractHash, _transferredValue);
                return this;
            }

            public ContractCallBuilder ByName(string name)
            {
                _invocationTarget = TransactionV1Target.StoredByName(name, _transferredValue);
                return this;
            }

            public ContractCallBuilder ByPackageHash(string contractHash, UInt32? version = null)
            {
                _invocationTarget = TransactionV1Target.StoredByPackageHash(contractHash, version, _transferredValue);
                return this;
            }

            public ContractCallBuilder ByPackageName(string name, UInt32? version = null)
            {
                _invocationTarget = TransactionV1Target.StoredByPackageName(name, version, _transferredValue);
                return this;
            }

            public ContractCallBuilder TransferredValue(ulong transferredValue)
            {
                _transferredValue = transferredValue;
                if (_invocationTarget is StoredTransactionV1Target storedTransactionV1Target)
                    storedTransactionV1Target.TransferredValue = transferredValue;
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
            private ulong _transferredValue = 0;
            private string _seed = null;
            private byte[] _wasm = null;

            public SessionBuilder()
            {
                _entryPoint = TransactionV1EntryPoint.Call;
            }

            public SessionBuilder Wasm(byte[] wasmBytes)
            {
                var target = TransactionV1Target.Session(wasmBytes, _transferredValue);
                target.IsInstallUpgrade = _isInstallOrUpgrade;
                target.Seed = _seed;
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

            public SessionBuilder TransferredValue(ulong transferredValue)
            {
                _transferredValue = transferredValue;
                if (_invocationTarget is SessionTransactionV1Target sessionTarget)
                    sessionTarget.TransferredValue = transferredValue;
                return this;
            }

            public SessionBuilder Seed(string seed)
            {
                _seed = seed;
                if (_invocationTarget is SessionTransactionV1Target sessionTarget)
                    sessionTarget.Seed = seed;
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