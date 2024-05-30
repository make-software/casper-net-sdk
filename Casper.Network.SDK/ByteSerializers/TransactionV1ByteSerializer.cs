using System;
using System.IO;
using Casper.Network.SDK.Types;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.ByteSerializers
{
    public class TransactionV1ByteSerializer : BaseByteSerializer, IByteSerializer<TransactionV1>
    {
        public byte[] ToBytes(TransactionTarget source)
        {
            var ms = new MemoryStream();

            WriteByte(ms, (byte)source.Type);

            if (source.Type == TransactionTargetType.Stored)
            {
                switch (source.Id)
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
                        WriteBytes(ms, Hex.Decode(byPackageHash.Addr));
                        WriteMaybeUInteger(ms, byPackageHash.Version);
                        break;
                    case ByPackageNameInvocationTarget byPackageName:
                        WriteByte(ms, (byte)InvocationTargetTag.ByPackageName);
                        WriteString(ms, byPackageName.Name);
                        WriteMaybeUInteger(ms, byPackageName.Version);
                        break;
                }
                WriteByte(ms, (byte)source.Runtime);
            }
            else if (source.Type == TransactionTargetType.Session)
            {
                WriteByte(ms, (byte)source.SessionKind);
                if (source.ModuleBytes == null || source.ModuleBytes.Length == 0)
                    WriteInteger(ms, 0);
                else
                {
                    WriteInteger(ms, source.ModuleBytes.Length);
                    WriteBytes(ms, source.ModuleBytes);
                }

                WriteByte(ms, (byte)source.Runtime);
            }

            return ms.ToArray();
        }

        public byte[] ToBytes(TransactionEntryPoint source)
        {
            var ms = new MemoryStream();

            if (source.Custom != null)
            {
                WriteByte(ms, 0x00);
                WriteString(ms, source.Custom);
            }
            else if (source.Native.HasValue)
            {
                WriteByte(ms, (byte)source.Native.Value);
            }
            else
            {
                throw new Exception("Cannot serialize empty TransactionEntryPoint to bytes");
            }

            return ms.ToArray();
        }

        public byte[] ToBytes(TransactionScheduling source)
        {
            var ms = new MemoryStream();

            WriteByte(ms, (byte)source.Type);

            if (source.Type == TransactionSchedulingType.FutureEra)
                WriteULong(ms, source.EraId);
            if (source.Type == TransactionSchedulingType.FutureTimestamp)
                WriteULong(ms, source.Timestamp);

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
            WriteBytes(ms, ToBytes(source.Scheduling));

            return ms.ToArray();
        }

        public byte[] ToBytes(PricingMode source)
        {
            var ms = new MemoryStream();

            if (source.Type == PricingModeType.Classic &&
                source.PaymentAmount.HasValue &&
                source.GasPriceTolerance.HasValue &&
                source.StandardPayment.HasValue)
            {
                ms.WriteByte((byte)PricingModeType.Classic);
                WriteULong(ms, (ulong)source.PaymentAmount.Value);
                WriteByte(ms, (byte)source.GasPriceTolerance.Value);
                WriteByte(ms, (byte)(source.StandardPayment.Value ? 0x01 : 0x00));
            }
            else if (source.Type == PricingModeType.Fixed &&
                     source.GasPriceTolerance.HasValue)
            {
                ms.WriteByte((byte)PricingModeType.Fixed);
                WriteByte(ms, (byte)source.GasPriceTolerance.Value);
            }
            else if (source.Type == PricingModeType.Reserved &&
                     source.Receipt != null)
            {
                ms.WriteByte((byte)PricingModeType.Reserved);
                WriteBytes(ms, Hex.Decode(source.Receipt));
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