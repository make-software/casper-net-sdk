namespace Casper.Network.SDK.ByteSerializers
{
    public interface IByteSerializer<T>
    {
        public byte[] ToBytes(T source);
    }
}