using System;
using System.IO;
using System.Linq;
using Casper.Network.SDK.Types;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.ByteSerializers
{
    public class ExecutableDeployItemByteSerializer : BaseByteSerializer, IByteSerializer<ExecutableDeployItem>
    {
        public byte[] ToBytes(ExecutableDeployItem source)
        {
            var ms = new MemoryStream();
            WriteByte(ms, source.Tag());

            if (source is ModuleBytesDeployItem)
            {
                var item = (ModuleBytesDeployItem) source;
                if(item.ModuleBytes == null || item.ModuleBytes.Length==0)
                    WriteInteger(ms, 0);
                else
                {
                    WriteInteger(ms, item.ModuleBytes.Length);
                    WriteBytes(ms, item.ModuleBytes);                    
                }
            }
            else if (source is StoredContractByHashDeployItem)
            {
                var item = (StoredContractByHashDeployItem) source;

                WriteBytes(ms, item.Hash);
                WriteString(ms, item.EntryPoint);
            }
            else if (source is StoredContractByNameDeployItem)
            {
                var item = (StoredContractByNameDeployItem) source;

                WriteString(ms, item.Name);
                WriteString(ms, item.EntryPoint);
            }
            else if (source is StoredVersionedContractByHashDeployItem)
            {
                var item = (StoredVersionedContractByHashDeployItem) source;

                WriteBytes(ms, item.Hash);
                // Version serializes as Option(U32). ie. 0x00 or 0x01xxxxxxxx
                if(item.Version == null) 
                    ms.WriteByte(0x00);
                else
                {
                    WriteByte(ms,0x01);
                    WriteUInteger(ms, (uint)item.Version);
                }
                WriteString(ms, item.EntryPoint);
            }
            else if (source is StoredVersionedContractByNameDeployItem)
            {
                var item = (StoredVersionedContractByNameDeployItem) source;

                WriteString(ms, item.Name);
                // Version serializes as Option(U32). ie. 0x00 or 0x01xxxxxxxx
                if(item.Version == null) 
                    ms.WriteByte(0x00);
                else
                {
                    WriteByte(ms,0x01);
                    WriteUInteger(ms, (uint)item.Version);
                }
                WriteString(ms, item.EntryPoint);
            }
            else if (source is TransferDeployItem)
            {
                var item = (TransferDeployItem) source;
            }

            var namedArgSerializer = new NamedArgByteSerializer();

            ms.Write(BitConverter.GetBytes(source.RuntimeArgs.Count));
            foreach (var args in source.RuntimeArgs)
                WriteBytes(ms, namedArgSerializer.ToBytes(args));

            return ms.ToArray();
        }
    }
}