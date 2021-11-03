namespace NetCasperSDK.Types
{
    public class CLResultTypeInfo : CLTypeInfo
    {
        public CLTypeInfo Ok { get; }

        public CLTypeInfo Err { get; }

        public CLResultTypeInfo(CLTypeInfo ok, CLTypeInfo err) : base(CLType.Result)
        {
            Ok = ok;
            Err = err;
        }
        
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return base.Equals(obj) && this.Ok.Equals(((CLResultTypeInfo) obj).Ok)
                   && this.Err.Equals(((CLResultTypeInfo) obj).Err);
        }
        
        public override int GetHashCode()
        {
            return ((int)Type^(int)Ok.Type)^(int)Err.Type;
        }
    }
}