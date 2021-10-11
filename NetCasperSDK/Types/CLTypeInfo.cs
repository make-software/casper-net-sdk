namespace NetCasperSDK.Types
{
    public class CLTypeInfo
    {
        public CLType Type { get; }

        public CLTypeInfo(CLType type)
        {
            Type = type;
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

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}