using System;

namespace Casper.Network.SDK.Types
{
    public class CLTypeInfo
    {
        public CLType Type { get; }

        public CLTypeInfo(CLType type)
        {
            Type = type;
        }
        
        public static implicit operator CLTypeInfo(CLType t)
        {
            return t switch
            {
                CLType.Bool => new CLTypeInfo(CLType.Bool),
                CLType.I32 => new CLTypeInfo(CLType.I32),
                CLType.I64 => new CLTypeInfo(CLType.I64),
                CLType.U8 => new CLTypeInfo(CLType.U8),
                CLType.U32 => new CLTypeInfo(CLType.U32),
                CLType.U64 => new CLTypeInfo(CLType.U64),
                CLType.U128 => new CLTypeInfo(CLType.U128),
                CLType.U256 => new CLTypeInfo(CLType.U256),
                CLType.U512 => new CLTypeInfo(CLType.U512),
                CLType.Unit => new CLTypeInfo(CLType.Unit),
                CLType.String => new CLTypeInfo(CLType.String),
                CLType.URef => new CLTypeInfo(CLType.URef),
                CLType.Any => new CLTypeInfo(CLType.Any),
                CLType.PublicKey => new CLTypeInfo(CLType.PublicKey),
                _ => throw new Exception($"Implicit cast not supported for CLType '{t}'.")
            };
        }


        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !GetType().Equals(obj.GetType()))
                return false;

            var that = (CLTypeInfo) obj;
            return Type == that.Type;
        }

        public override int GetHashCode()
        {
            return (int) Type;
        }

        /// <summary>
        /// Returns true if obj can be part of the same CLValue.List. 
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>true if obj can be part of the same CLValue list.</returns>
        public virtual bool IsListCompatibleWith(object obj)
        {
            return this.Equals(obj);
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}