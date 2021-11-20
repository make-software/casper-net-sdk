namespace Casper.Network.SDK.Types
{

    
    public class CLKeyTypeInfo : CLTypeInfo
    {
        public KeyIdentifier KeyIdentifier { get; }

        public CLKeyTypeInfo(KeyIdentifier keyIdentifier)
            : base(CLType.Key)
        {
            KeyIdentifier = keyIdentifier;
        }
        
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return base.Equals(obj) && this.KeyIdentifier.Equals(((CLKeyTypeInfo) obj).KeyIdentifier);
        }
        
        public override int GetHashCode()
        {
            return (int)Type^(int)KeyIdentifier;
        }
    }
}