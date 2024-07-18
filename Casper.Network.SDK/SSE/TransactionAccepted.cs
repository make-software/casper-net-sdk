using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.SSE
{
    [JsonConverter(typeof(Transaction.TransactionConverter))]
    public class TransactionAccepted : Transaction
    {
    }
}