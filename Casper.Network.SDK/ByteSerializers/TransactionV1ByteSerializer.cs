using System;
using System.IO;
using Casper.Network.SDK.Types;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.ByteSerializers
{
    public class TransactionV1ByteSerializer : BaseByteSerializer, IByteSerializer<TransactionV1>
    {
        public byte[] ToBytes(ITransactionV1Target source)
        {
            var ms = new MemoryStream();

            switch (source)
            {
                case NativeTransactionV1Target:
                    WriteByte(ms, (byte)TransactionTargetType.Native);
                    break;
                case StoredTransactionV1Target storedTarget:
                    WriteByte(ms, (byte)TransactionTargetType.Stored);
                    switch (storedTarget.Id)
                    {
                        case ByHashInvocationTarget byHash:
                            WriteByte(ms, (byte)InvocationTargetTag.ByHash);
                            WriteBytes(ms, Hex.Decode(byHash.Hash));
                            break;
                        case ByNameInvocationTarget byName:
                            WriteByte(ms, (byte)InvocationTargetTag.ByName);
                            WriteString(ms, byName.Name);
                            break;
                        case ByPackageHashInvocationTarget byPackageHash:
                            WriteByte(ms, (byte)InvocationTargetTag.ByPackageHash);
                            WriteBytes(ms, Hex.Decode(byPackageHash.Hash));
                            WriteMaybeUInteger(ms, byPackageHash.Version);
                            break;
                        case ByPackageNameInvocationTarget byPackageName:
                            WriteByte(ms, (byte)InvocationTargetTag.ByPackageName);
                            WriteString(ms, byPackageName.Name);
                            WriteMaybeUInteger(ms, byPackageName.Version);
                            break;
                    }
                    WriteByte(ms, (byte)storedTarget.Runtime);
                    break;
                case SessionTransactionV1Target sessionTarget:
                {
                    WriteByte(ms, (byte)TransactionTargetType.Session);
                
                    if (sessionTarget.ModuleBytes == null || sessionTarget.ModuleBytes.Length == 0)
                        WriteInteger(ms, 0);
                    else
                    {
                        WriteInteger(ms, sessionTarget.ModuleBytes.Length);
                        WriteBytes(ms, sessionTarget.ModuleBytes);
                    }

                    WriteByte(ms, (byte)sessionTarget.Runtime);
                    break;
                }
            }

            return ms.ToArray();
        }

        public byte[] ToBytes(ITransactionV1EntryPoint source)
        {
            var ms = new MemoryStream();

            switch (source)
            {
                case CustomTransactionV1EntryPoint customEntryPoint:
                    WriteByte(ms, 0x00);
                    WriteString(ms, customEntryPoint.Name);
                    break;
                case NativeTransactionV1EntryPoint nativeEntryPoint:
                    WriteByte(ms, (byte)nativeEntryPoint.Type);
                    break;
                default:
                    throw new Exception("Cannot serialize empty TransactionEntryPoint to bytes");
            }

            return ms.ToArray();
        }

        public byte[] ToBytes(ITransactionV1Scheduling source)
        {
            var ms = new MemoryStream();

            switch (source)
            {
                case StandardTransactionV1Scheduling:
                    WriteByte(ms, (byte)TransactionV1SchedulingType.Standard);
                    break;
                case FutureEraTransactionV1Scheduling eraScheduling:
                    WriteByte(ms, (byte)TransactionV1SchedulingType.FutureEra);
                    WriteULong(ms, eraScheduling.EraId);
                    break;
                case FutureTimestampTransactionV1Scheduling timestampScheduling:
                    WriteByte(ms, (byte)TransactionV1SchedulingType.FutureTimestamp);
                    WriteULong(ms, timestampScheduling.Timestamp);
                    break;
            }

            return ms.ToArray();
        }

        public byte[] ToBytes(TransactionV1Body source)
        {
            var ms = new MemoryStream();

            var namedArgSerializer = new NamedArgByteSerializer();

            ms.Write(BitConverter.GetBytes(source.RuntimeArgs.Count));
            foreach (var args in source.RuntimeArgs)
                WriteBytes(ms, namedArgSerializer.ToBytes(args));

            WriteBytes(ms, ToBytes(source.Target));
            WriteBytes(ms, ToBytes(source.EntryPoint));
            WriteByte(ms, (byte)source.Category);
            WriteBytes(ms, ToBytes(source.Scheduling));

            return ms.ToArray();
        }

        public byte[] ToBytes(IPricingMode source)
        {
            var ms = new MemoryStream();

            switch (source)
            {
                case ClassicPricingMode classicPricingMode:
                    ms.WriteByte((byte)PricingModeType.Classic);
                    WriteULong(ms, (ulong)classicPricingMode.PaymentAmount);
                    WriteByte(ms, (byte)classicPricingMode.GasPriceTolerance);
                    WriteByte(ms, (byte)(classicPricingMode.StandardPayment ? 0x01 : 0x00));
                    break;
                case FixedPricingMode fixedPricingMode:
                    ms.WriteByte((byte)PricingModeType.Fixed);
                    WriteByte(ms, (byte)fixedPricingMode.GasPriceTolerance);
                    break;
                case ReservedPricingMode reservedPricingMode:
                    ms.WriteByte((byte)PricingModeType.Reserved);
                    WriteBytes(ms, Hex.Decode(reservedPricingMode.Receipt));
                    break;
            }

            return ms.ToArray();
        }

        public byte[] ToBytes(InitiatorAddr source)
        {
            var ms = new MemoryStream();

            if (source.PublicKey != null)
            {
                WriteByte(ms, 0x00);
                WriteBytes(ms, source.PublicKey.GetBytes());
            }
            else if (source.AccountHash != null)
            {
                WriteByte(ms, 0x01);
                WriteBytes(ms, source.AccountHash.RawBytes);
            }

            return ms.ToArray();
        }

        public byte[] ToBytes(TransactionV1Header source)
        {
            var ms = new MemoryStream();
            WriteString(ms, source.ChainName);
            WriteULong(ms, source.Timestamp);
            WriteULong(ms, source.Ttl);
            WriteBytes(ms, Hex.Decode(source.BodyHash));
            WriteBytes(ms, ToBytes(source.PricingMode));
            WriteBytes(ms, ToBytes(source.InitiatorAddr));
            return ms.ToArray();
        }

        public byte[] ToBytes(TransactionV1 source)
        {
            var ms = new MemoryStream();

            WriteBytes(ms, Hex.Decode(source.Hash));

            WriteBytes(ms, ToBytes(source.Header));

            WriteBytes(ms, ToBytes(source.Body));
            // add the approvals
            //
            var approvalSerializer = new DeployApprovalByteSerializer();
            WriteInteger(ms, source.Approvals.Count);
            foreach (var approval in source.Approvals)
                WriteBytes(ms, approvalSerializer.ToBytes(approval));

            return ms.ToArray();
        }
    }
}