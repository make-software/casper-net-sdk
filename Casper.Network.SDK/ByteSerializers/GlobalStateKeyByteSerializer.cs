using System.IO;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.ByteSerializers
{
    public class GlobalStateKeyByteSerializer : BaseByteSerializer, IByteSerializer<GlobalStateKey>
    {
        public byte[] ToBytes(GlobalStateKey source)
        {
            var ms = new MemoryStream();

            WriteByte(ms, (byte) source.KeyIdentifier);
            WriteBytes(ms, source.RawBytes);

            if (source is URef uref)
                WriteByte(ms, (byte) uref.AccessRights);
            
            return ms.ToArray();
        }
    }
}