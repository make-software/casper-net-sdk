using System.IO;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.ByteSerializers
{
    public class DeployApprovalByteSerializer : BaseByteSerializer, IByteSerializer<DeployApproval>
    {
        public byte[] ToBytes(DeployApproval source)
        {
            var ms = new MemoryStream();
            WriteBytes(ms, source.Signer.GetBytes());
            WriteBytes(ms, source.Signature.GetBytes());
            return ms.ToArray();
        }
    }
}