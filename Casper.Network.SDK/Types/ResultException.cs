using System;
using System.IO;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class ResultException : Exception
    {
        public byte[] Bytes { get; init; }
        
        public object ErrorValue { get; init; }
        
        public ResultException(byte[] bytes, object err, CLType okType, CLType errType) 
            : base($"Result({okType},{errType}) variable contains an error.")
        {
            Bytes = bytes;
            ErrorValue = err;
        }

        public ResultException(byte[] bytes, CLType okType, CLType errType) 
            : base($"Cannot convert {Hex.ToHexString(bytes[1..])}' to '{errType}'.")
        {
            Bytes = bytes;
            ErrorValue = null;
        }
    }
}