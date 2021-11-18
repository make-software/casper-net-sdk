namespace Casper.Network.SDK.Types
{
    public enum KeyTag
    {
        Account = 0x00,
        Hash = 0x01,
        URef = 0x02,
        Transfer = 0x03,
        DeployInfo = 0x04,
        EraInfo = 0x05,
        Balance = 0x06,
        Bid = 0x07,
        Withdraw = 0x08
    }
    
    public class CLKeyTypeInfo : CLTypeInfo
    {
        public KeyTag KeyTag { get; }

        public CLKeyTypeInfo(KeyTag keyTag)
            : base(CLType.Key)
        {
            KeyTag = keyTag;
        }
        
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return base.Equals(obj) && this.KeyTag.Equals(((CLKeyTypeInfo) obj).KeyTag);
        }
        
        public override int GetHashCode()
        {
            return (int)Type^(int)KeyTag;
        }
    }
}