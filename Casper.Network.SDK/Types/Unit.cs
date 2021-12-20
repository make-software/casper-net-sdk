namespace Casper.Network.SDK.Types
{
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