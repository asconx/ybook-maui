    using System.Text.Json.Serialization;

    namespace yBook.Models
    {
        /// <summary>
        /// Response wrapper for blockade API
        /// </summary>
        public class ICallendarApiResponse
        {
            public List<ICallendarDto> Items { get; set; } = new();
        }

        /// <summary>
        /// Blockade data from API
        /// </summary>
        public class ICallendarDto
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("organization_id")]
            public int OrganizationId { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
            
            [JsonPropertyName("area")]
            public int Area { get; set; }
            [JsonPropertyName("color")]
            public string Color { get; set; }

            [JsonPropertyName("is_available")]
            public string IsAvailable { get; set; }

            [JsonPropertyName("lock_id")]
            public int LockId { get; set; }

            [JsonPropertyName("amenities")]
            public ICallendarRoomAmenities Amenities { get; set; }

            [JsonPropertyName("beds")]
            public ICallendarRoomBeds Beds { get; set; }

            [JsonPropertyName("calendar_position")]
            public int CalendarPosition { get; set; }
            [JsonPropertyName("date_modified")]
            public string DateModified {  get; set; }
            [JsonPropertyName("default_price")]
            public int DefaultPrice { get; set; }
            [JsonPropertyName("max_number_of_people")]
            public int MaxNumberOfPeople { get; set; }
            [JsonPropertyName("min_number_of_people")]
            public int MinNumberOfPeople { get; set; }
            [JsonPropertyName("photos")]
            public ICallendarRoomPhotos Photos { get; set; }
            [JsonPropertyName("property_id")]
            public int PropertyId { get; set; }
            [JsonPropertyName("short_name")]
            public string ShortName { get; set; }
            [JsonPropertyName("type")]
            public int Type { get; set; }
    }

    public class ICallendarRoomAmenities 
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }
    }
    public class ICallendarRoomBeds
             
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }
    }
    public class ICallendarRoomPhotos 
    {
        [JsonPropertyName ("id")]
        public int Id { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
    }
}
