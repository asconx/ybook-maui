    using System.Text.Json.Serialization;

    namespace yBook.Models
    {
        /// <summary>
        /// Response wrapper for blockade API
        /// </summary>
        public class BlockadeApiResponse
        {
            public List<BlockadeDto> Items { get; set; } = new();
        }

        /// <summary>
        /// Blockade data from API
        /// </summary>
        public class BlockadeDto
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("organization_id")]
            public int OrganizationId { get; set; }

            [JsonPropertyName("date_modified")]
            public string DateModified { get; set; }

            [JsonPropertyName("date_created")]
            public string DateCreated { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("notes")]
            public string Notes { get; set; }

            [JsonPropertyName("apply_from_date")]
            public string ApplyFromDate { get; set; }

            [JsonPropertyName("apply_to_date")]
            public string ApplyToDate { get; set; }

            [JsonPropertyName("rooms")]
            public List<BlockadeRoomDto> Rooms { get; set; } = new();
        }

        /// <summary>
        /// Room data from blockade API
        /// </summary>
        public class BlockadeRoomDto
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("full_name")]
            public string FullName { get; set; }

            [JsonPropertyName("short_name")]
            public string ShortName { get; set; }

            [JsonPropertyName("room_id")]
            public int RoomId { get; set; }
        }
    }
