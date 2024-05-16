using System;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public enum BidAddrTag
    {
        /// <summary>
        /// Unified BidAddr (refers to validator address).
        /// </summary>
        Unified = 0, //legacy type
        /// <summary>
        /// Validator BidAddr.
        /// </summary>
        Validator = 1,
        /// <summary>
        /// Delegator BidAddr,
        /// </summary>
        Delegator = 2,
    }
    
    public class BidAddrKey : GlobalStateKey
    {
        public static string KEYPREFIX = "bid-addr-";

        public BidAddrTag Tag { get; init; }

        public AccountHashKey Validator
        {
            get
            {
                var hash = Tag == BidAddrTag.Delegator ? Key.Substring(0,32) : Key;
                return new AccountHashKey("account-hash-" + hash);
            }
        }
        
        public AccountHashKey Delegator
        {
            get
            {
                var hash = Tag == BidAddrTag.Delegator ? Key.Substring(32) : null;
                return hash != null 
                    ? new AccountHashKey("account-hash-" + hash)
                    : null;
            }
        }
        
        public BidAddrKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.BidAddr;
            var bytes = Hex.Decode(Key);
            if (bytes.Length <= 0)
                throw new Exception("Wrong key length.");
            switch (bytes[0])
            {
                case (byte)BidAddrTag.Unified:
                    Tag = BidAddrTag.Unified;
                    if (bytes.Length != 33)
                        throw new Exception("Wrong key length for Unified BidAddr. Expected 33 bytes.");
                    break;
                case (byte)BidAddrTag.Validator:
                    Tag = BidAddrTag.Validator;
                    if (bytes.Length != 33)
                        throw new Exception("Wrong key length for Validator BidAddr. Expected 33 bytes.");
                    break;
                case (byte)BidAddrTag.Delegator:
                    Tag = BidAddrTag.Delegator;
                    if (bytes.Length != 65)
                        throw new Exception("Wrong key length for Unified BidAddr. Expected 65 bytes.");
                    break;
                default:
                    throw new Exception($"Wrong BidAddr tag '{bytes[0]}'.");
            }
        }

        public BidAddrKey(byte[] key) : this(KEYPREFIX + CEP57Checksum.Encode(key))
        {
        }
    }
}