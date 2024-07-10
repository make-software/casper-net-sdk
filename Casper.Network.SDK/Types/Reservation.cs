using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Container for bytes recording location, type and data for a gas reservation.
    /// </summary>
    public class Reservation
    {
        [JsonPropertyName("receipt")]
        public string Receipt { get; init; }

        [JsonPropertyName("reservation_kind")]
        public byte ReservationKind { get; init; }
        
        [JsonPropertyName("reservation_data")]
        public string ReservationData { get; init; }
    }
}