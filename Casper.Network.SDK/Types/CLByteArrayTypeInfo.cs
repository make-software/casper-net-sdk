using System;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A `CLTypeInfo` specific for byte arrays.
    /// </summary>
    public class CLByteArrayTypeInfo : CLTypeInfo
    {
        public int Size { get; }

        public CLByteArrayTypeInfo(int size)
            : base(CLType.ByteArray)
        {
            Size = size;
        }
        
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            return base.Equals(obj) && ((CLByteArrayTypeInfo) obj).Size == this.Size;
        }
        
        public override int GetHashCode()
        {
            return (int)Type^Size;
        }

        public override Type GetFrameworkType()
        {
            return typeof(byte[]);
        }
    }
}