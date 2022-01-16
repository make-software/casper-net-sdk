namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Represents the `unit` Rust type. aka `()`. It  is used when there is no other
    /// meaningful value that could be returned.
    /// </summary>
    public class Unit
    {
        public static readonly Unit Default = new Unit();

        protected Unit()
        {
        }
        
        public override bool Equals(object obj)
        {
            return obj is Unit;
        }
        
        public override int GetHashCode()
        {
            return 0;
        }
    }
}