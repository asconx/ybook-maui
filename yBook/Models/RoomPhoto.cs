using System.Text.Json.Serialization;

namespace yBook.Models
{
    public class RoomPhoto
    {
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [JsonPropertyName("date_modified")]
        public string? DateModified { get; set; }

        [JsonPropertyName("file_id")]
        public int FileId { get; set; }
    }
}
