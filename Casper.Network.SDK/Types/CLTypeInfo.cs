namespace Casper.Network.SDK.Types
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

        /// <summary>
        /// Returns true if obj can be part of the same CLValue.List. 
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>true if obj can be part of the same CLValue list.</returns>
        public virtual bool IsListCompatibleWith(object obj)
        {
            return this.Equals(obj);
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}