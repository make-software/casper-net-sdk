namespace NetCasperSDK.Types
{
    public class CLOptionTypeInfo : CLTypeInfo
    {
        public CLTypeInfo OptionType { get; }

        public CLOptionTypeInfo(CLTypeInfo type)
            : base(CLType.Option)
        {
            OptionType = type;
        }
        
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return base.Equals(obj) && this.OptionType.Equals(((CLOptionTypeInfo) obj).OptionType);
        }
        
        public override int GetHashCode()
        {
            return (int)Type^(int)OptionType.Type;
        }
        
        
    }
}