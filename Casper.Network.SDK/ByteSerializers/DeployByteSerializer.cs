using System;
using System.IO;
using Casper.Network.SDK.Types;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.ByteSerializers
{
    public class DeployByteSerializer : BaseByteSerializer, IByteSerializer<Deploy>
    {
        public byte[] ToBytes(DeployHeader source)
        {
            var ms = new MemoryStream();

            // serialize the header
            //
            WriteBytes(ms, source.Account.GetBytes());
            
            WriteULong(ms, source.Timestamp);

            WriteULong(ms, source.Ttl);

            WriteULong(ms, source.GasPrice);
            
            WriteBytes(ms, Hex.Decode(source.BodyHash));
            
            WriteInteger(ms, source.Dependencies.Count);
            foreach (var dependency in source.Dependencies)
                WriteBytes(ms, Hex.Decode(dependency));

            WriteString(ms, source.ChainName);

            return ms.ToArray();
        }
        
        public byte[] ToBytes(Deploy source)
        {
            var itemSerializer = new ExecutableDeployItemByteSerializer();
            var approvalSerializer = new DeployApprovalByteSerializer();
            
            var ms = new MemoryStream();

            WriteBytes(ms, ToBytes(source.Header));
            
            WriteBytes(ms, Hex.Decode(source.Hash));

            WriteBytes(ms, itemSerializer.ToBytes(source.Payment));
            
            WriteBytes(ms, itemSerializer.ToBytes(source.Session));
            
            // add the approvals
            //
            WriteInteger(ms, source.Approvals.Count);
            foreach (var approval in source.Approvals)
                WriteBytes(ms, approvalSerializer.ToBytes(approval));

            return ms.ToArray();
        }
    }
}