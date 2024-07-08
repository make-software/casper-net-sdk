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
        /// <summary>
        /// BidAddr for auction credit.
        /// </summary>
        Credit = 4,
    }
    
    public class BidAddrKey : GlobalStateKey
    {
        public static string KEYPREFIX = "bid-addr-";

        public BidAddrTag Tag { get; init; }

        /// <summary>
        /// Unified BidAddr.
        /// </summary>
        public AccountHashKey Unified { get; init; }
        
        /// <summary>
        /// The valicator address.
        /// </summary>
        public AccountHashKey Validator { get; init; }
        
        /// <summary>
        /// The delegator address.
        /// </summary>
        public AccountHashKey Delegator { get; init; }
        
        /// <summary>
        /// The era id.
        /// </summary>
        public ulong EraId { get; init; }
        
        public BidAddrKey(string key) : base(key, KEYPREFIX)
        {
            KeyIdentifier = KeyIdentifier.BidAddr;
            var bytes = Hex.Decode(key.Substring(key.LastIndexOf('-') + 1));
            
            if (bytes.Length == 0)
                throw new Exception("Wrong key length.");
            
            switch (bytes[0])
            {
                case (byte)BidAddrTag.Unified:
                    Tag = BidAddrTag.Unified;
                    if (bytes.Length != 33)
                        throw new Exception("Wrong key length for Unified BidAddr. Expected 33 bytes.");
                    Unified = new AccountHashKey(bytes.Slice(1, 33));
                    break;
                case (byte)BidAddrTag.Validator:
                    Tag = BidAddrTag.Validator;
                    if (bytes.Length != 33)
                        throw new Exception("Wrong key length for Validator BidAddr. Expected 33 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    break;
                case (byte)BidAddrTag.Delegator:
                    Tag = BidAddrTag.Delegator;
                    if (bytes.Length != 65)
                        throw new Exception("Wrong key length for Unified BidAddr. Expected 65 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    Delegator = new AccountHashKey(bytes.Slice(33));
                    break;
                case (byte)BidAddrTag.Credit:
                    Tag = BidAddrTag.Credit;
                    if (bytes.Length != 41)
                        throw new Exception("Wrong key length for Credit BidAddr. Expected 41 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    EraId = BitConverterExtensions.ToUInt64(bytes.Slice(33));
                    break;
                default:
                    throw new Exception($"Wrong BidAddr tag '{bytes[0]}'.");
            }
        }

        public BidAddrKey(byte[] key) : this(KEYPREFIX + Hex.ToHexString(key))
        {
        }
    }
}
