using System.Text.Json.Serialization;

namespace yBook.Models
{
    public class Stay
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("reservation_id")]
        public int ReservationId { get; set; }

        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [JsonPropertyName("checkin_date")]
        public string CheckinDate { get; set; }

        [JsonPropertyName("checkout_date")]
        public string CheckoutDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("number_of_guests")]
        public int NumberOfGuests { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }
    }

    public class StayResponse
    {
        [JsonPropertyName("items")]
        public Stay[] Items { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
