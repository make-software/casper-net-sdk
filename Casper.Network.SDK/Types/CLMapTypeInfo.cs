namespace Casper.Network.SDK.Types
{
    public class CLMapTypeInfo : CLTypeInfo
    {
        public CLTypeInfo KeyType { get; }

        public CLTypeInfo ValueType { get; }
        
        public CLMapTypeInfo(CLTypeInfo keyType, CLTypeInfo valueType) : base(CLType.Map)
        {
            KeyType = keyType;
            ValueType = valueType;
        }
        
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return base.Equals(obj) && this.KeyType.Equals(((CLMapTypeInfo) obj).KeyType)
                                    && this.ValueType.Equals(((CLMapTypeInfo) obj).ValueType);
        }
        
        public override int GetHashCode()
        {
            return ((int)Type^(int)KeyType.Type)^(int)ValueType.Type;
        }
    }
}
