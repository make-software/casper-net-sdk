namespace Casper.Network.SDK.Types
{
    public class TransactionHash
    {
        public string Deploy { get; init; }
        
        public string Version1 { get; init; }

        public override string ToString()
        {
            return Deploy ?? Version1;
        }
    }
}
