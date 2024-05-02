using System;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Exception thrown during CLValue conversion errors.
    /// </summary>
    public class CLValueException : Exception
    {
        public byte[] Bytes { get; init; }
        
        public object ErrorValue { get; init; }
        
        public CLValueException(byte[] bytes, object err, CLTypeInfo okType, CLTypeInfo errType) 
            : base($"Result({okType},{errType}) variable contains an error.")
        {
            Bytes = bytes;
            ErrorValue = err;
        }

        public CLValueException(byte[] bytes, CLTypeInfo okType, CLTypeInfo errType) 
            : base($"Cannot convert {Hex.ToHexString(bytes.Slice(1))}' to '{errType}'.")
        {
            Bytes = bytes;
            ErrorValue = null;
        }
    }
}