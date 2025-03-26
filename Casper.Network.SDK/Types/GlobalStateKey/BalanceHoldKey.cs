using System;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public enum BalanceHoldTag
    {
        /// <summary>
        /// Tag for gas variant.
        /// </summary>
        Gas = 0,
        /// <summary>
        /// Tag for processing variant.
        /// </summary>
        Processing = 1,
    }
    
    public class BalanceHoldKey : GlobalStateKey
    {
        private const string KEYPREFIX = "balance-hold-";
        public static string KeyPrefix => KEYPREFIX;
        
        public BalanceHoldTag Tag { get; init; }

        public URef URef => new URef(RawBytes.Slice(1,33), AccessRights.NONE);

        public UInt64 BlockTime
        {
            get
            {
                var bytes = RawBytes.Slice(33);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return BitConverterExtensions.ToUInt64(bytes);
            }
        }
            
        public BalanceHoldKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.BalanceHold;
            if (RawBytes.Length <= 0)
                throw new Exception("Wrong key length.");
            switch (RawBytes[0])
            {
                case (byte)BalanceHoldTag.Gas:
                    Tag = BalanceHoldTag.Gas;
                    break;
                case (byte)BalanceHoldTag.Processing:
                    Tag = BalanceHoldTag.Processing;
                    break;
                default:
                    throw new Exception($"Wrong BalanceHold tag '{RawBytes[0]}'.");
            }
            
            if (RawBytes.Length != 41) // tag + purse uref (32) + block time (8)
                throw new Exception("Wrong key length for BalanceHold. Expected 41 bytes.");
        }

        public BalanceHoldKey(byte[] key) : this(KEYPREFIX + Hex.ToHexString(key))
        {
        }
    }
}
