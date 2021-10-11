namespace NetCasperSDK.ByteSerializers
{
    public interface IByteSerializer<T>
    {
        public byte[] ToBytes(T source);
    }
}