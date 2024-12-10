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
        DelegatedAccount = 2,
        /// <summary>
        /// BidAddr for delegated purse bid.
        /// </summary>
        DelegatedPurse = 3,
        /// <summary>
        /// BidAddr for auction credit.
        /// </summary>
        Credit = 4,
        /// <summary>
        /// BidAddr for reserved delegation account bid.
        /// </summary>
        ReservedDelegationAccount = 5,
        /// <summary>
        /// BidAddr for reserved delegation purse bid.
        /// </summary>
        ReservedDelegationPurse = 6,
        /// <summary>
        /// BidAddr for unbonding accounts.
        /// </summary>
        UnbondAccount = 7,
        /// <summary>
        /// BidAddr for unbonding purses.
        /// </summary>
        UnbondPurse = 8,
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
        public AccountHashKey DelegatorAccount { get; init; }
        
        /// <summary>
        /// The delegator purse address.
        /// </summary>
        public string DelegatorPurseAddress { get; init; }
        
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
                case (byte)BidAddrTag.DelegatedAccount:
                    Tag = BidAddrTag.DelegatedAccount;
                    if (bytes.Length != 65)
                        throw new Exception("Wrong key length for DelegatedAccount BidAddr. Expected 65 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    DelegatorAccount = new AccountHashKey(bytes.Slice(33));
                    break;
                case (byte)BidAddrTag.DelegatedPurse:
                    Tag = BidAddrTag.DelegatedPurse;
                    if (bytes.Length != 65)
                        throw new Exception("Wrong key length for DelegatedPurse BidAddr. Expected 65 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    DelegatorPurseAddress = Hex.ToHexString(bytes.Slice(33));
                    break;
                case (byte)BidAddrTag.Credit:
                    Tag = BidAddrTag.Credit;
                    if (bytes.Length != 41)
                        throw new Exception("Wrong key length for Credit BidAddr. Expected 41 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    EraId = BitConverterExtensions.ToUInt64(bytes.Slice(33));
                    break;
                case (byte)BidAddrTag.ReservedDelegationAccount:
                    Tag = BidAddrTag.ReservedDelegationAccount;
                    if (bytes.Length != 65)
                        throw new Exception("Wrong key length for ReservedDelegationAccount BidAddr. Expected 65 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    DelegatorAccount = new AccountHashKey(bytes.Slice(33));
                    break;
                case (byte)BidAddrTag.ReservedDelegationPurse:
                    Tag = BidAddrTag.ReservedDelegationPurse;
                    if (bytes.Length != 65)
                        throw new Exception("Wrong key length for ReservedDelegationPurse BidAddr. Expected 65 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    DelegatorPurseAddress = Hex.ToHexString(bytes.Slice(33));
                    break;
                case (byte)BidAddrTag.UnbondAccount:
                    Tag = BidAddrTag.UnbondAccount;
                    if (bytes.Length != 65)
                        throw new Exception("Wrong key length for UnbondAccount BidAddr. Expected 65 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    DelegatorAccount = new AccountHashKey(bytes.Slice(33));
                    break;
                case (byte)BidAddrTag.UnbondPurse:
                    Tag = BidAddrTag.UnbondPurse;
                    if (bytes.Length != 65)
                        throw new Exception("Wrong key length for UnbondPurse BidAddr. Expected 65 bytes.");
                    Validator = new AccountHashKey(bytes.Slice(1, 33));
                    DelegatorPurseAddress = Hex.ToHexString(bytes.Slice(33));
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
