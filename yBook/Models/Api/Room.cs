using System.Text.Json.Serialization;

namespace yBook.Models.Api
{
    /// <summary>
    /// Model odpowiadający strukturze pojedynczego pokoju zwracanego przez API
    /// Rozszerzony o dodatkowe pola zgodne z przykładową odpowiedzią
    /// </summary>
    public class Room
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("property_id")]
        public int PropertyId { get; set; }

        [JsonPropertyName("date_modified")]
        public string DateModified { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("short_name")]
        public string? ShortName { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("area")]
        public string? Area { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("default_price")]
        public decimal DefaultPrice { get; set; }

        [JsonPropertyName("is_available")]
        public int IsAvailable { get; set; }

        [JsonPropertyName("max_number_of_people")]
        public int MaxNumberOfPeople { get; set; }

        [JsonPropertyName("min_number_of_people")]
        public int MinNumberOfPeople { get; set; }

        [JsonPropertyName("calendar_position")]
        public int CalendarPosition { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("amenities")]
        public List<Amenity>? Amenities { get; set; }

        [JsonPropertyName("photos")]
        public List<Photo>? Photos { get; set; }

        [JsonPropertyName("beds")]
        public List<Bed>? Beds { get; set; }
    }

    public class Amenity
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }
    }

    public class Photo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }
    }

    public class Bed
    {
        // struktura beds nieznana w pełni, zostawiamy miejsce na rozszerzenie
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
