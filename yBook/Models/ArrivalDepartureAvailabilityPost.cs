using System.Text.Json.Serialization;

namespace yBook.Models
{
    public class ArrivalDepartureAvailabilityPost
    {
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [JsonPropertyName("day")]
        public string Day { get; set; }

        [JsonPropertyName("can_arrive")]
        public bool CanArrive { get; set; }

        [JsonPropertyName("can_depart")]
        public bool CanDepart { get; set; }
    }
}
