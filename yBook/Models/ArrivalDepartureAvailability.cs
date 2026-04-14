using System;
using System.Text.Json.Serialization;

namespace yBook.Models
{
    public class ArrivalDepartureAvailability
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("date_modified")]
        public string DateModified { get; set; }

        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [JsonPropertyName("day")]
        public string Day { get; set; }

        [JsonPropertyName("can_arrive")]
        public int CanArrive { get; set; }

        [JsonPropertyName("can_depart")]
        public int CanDepart { get; set; }
    }

    public class ArrivalDepartureAvailabilityResponse
    {
        [JsonPropertyName("items")]
        public ArrivalDepartureAvailability[] Items { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
