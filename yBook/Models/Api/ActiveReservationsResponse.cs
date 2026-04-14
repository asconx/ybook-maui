using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace yBook.Models.Api
{
    public class ActiveReservationsResponse
    {
        [JsonPropertyName("today")]
        public string Today { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<ActiveReservationItem>? Items { get; set; }

        [JsonPropertyName("checkedInReservations")]
        public List<object>? CheckedInReservations { get; set; }

        [JsonPropertyName("reservationIds")]
        public List<int>? ReservationIds { get; set; }
    }

    public class ActiveReservationItem
    {
        [JsonPropertyName("reservation")]
        public ReservationDto? Reservation { get; set; }

        [JsonPropertyName("room")]
        public RoomDto? Room { get; set; }

        [JsonPropertyName("client")]
        public ClientDto? Client { get; set; }
    }

    public class ReservationDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("date_from")] public string? DateFrom { get; set; }
        [JsonPropertyName("date_to")] public string? DateTo { get; set; }
        [JsonPropertyName("is_checked_in")]
        [JsonConverter(typeof(BoolIntJsonConverter))]
        public bool IsCheckedIn { get; set; }

        [JsonPropertyName("is_paid")]
        [JsonConverter(typeof(BoolIntJsonConverter))]
        public bool IsPaid { get; set; }

        [JsonPropertyName("is_until_today")]
        [JsonConverter(typeof(BoolIntJsonConverter))]
        public bool IsUntilToday { get; set; }

        [JsonPropertyName("status_id")] public int StatusId { get; set; }
    }

    public class RoomDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("short_name")] public string? ShortName { get; set; }
    }

    public class ClientDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("phone")] public string? Phone { get; set; }
    }
}
