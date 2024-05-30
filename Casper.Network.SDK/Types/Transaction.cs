namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// A versioned wrapper for a transaction or deploy.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// The deploy.
        /// </summary>
        public Deploy Deploy { get; init; }
        
        /// <summary>
        /// A version 1 transaction.
        /// </summary>
        public TransactionV1 TransactionV1 { get; init; }
    }
}