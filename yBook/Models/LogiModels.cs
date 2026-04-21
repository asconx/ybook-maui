using System.Text.Json.Serialization;
using System.Text.Json;

namespace yBook.Models
{
    public enum TypAkcji
    {
        Logowanie, Wylogowanie, Dodanie, Edycja, Usuniecie, Eksport, Import, Inne
    }

    public class LogAkcji
    {
        public int Id { get; init; }
        public DateTime Data { get; init; }
        public string DostepneDane { get; init; } = "";
        public string Uzytkownik { get; init; } = "";
        public TypAkcji Typ { get; init; }
        public string Opis { get; init; } = "";
        public int ItemId { get; init; }

        public string DataStr => Data.ToString("dd.MM.yyyy HH:mm");
        public string TypLabel => Typ switch
        {
            TypAkcji.Logowanie => "Logowanie",
            TypAkcji.Wylogowanie => "Wylogowanie",
            TypAkcji.Dodanie => "Dodanie",
            TypAkcji.Edycja => "Edycja",
            TypAkcji.Usuniecie => "Usunięcie",
            TypAkcji.Eksport => "Eksport",
            TypAkcji.Import => "Import",
            _ => "Inne"
        };
        public string TypKolor => Typ switch
        {
            TypAkcji.Logowanie => "#4CAF50",
            TypAkcji.Wylogowanie => "#FF9800",
            TypAkcji.Dodanie => "#2196F3",
            TypAkcji.Edycja => "#9C27B0",
            TypAkcji.Usuniecie => "#F44336",
            TypAkcji.Eksport => "#00BCD4",
            TypAkcji.Import => "#607D8B",
            _ => "#9E9E9E"
        };
    }

    public class SpaceDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] Formats = {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ"
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString() ?? "";
            foreach (var fmt in Formats)
                if (DateTime.TryParseExact(str, fmt,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
                    return dt;
            return DateTime.TryParse(str, out var fallback) ? fallback : DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    public class UserDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    public class LogAkcjiDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("date_modified")]
        [JsonConverter(typeof(SpaceDateTimeConverter))]
        public DateTime DateModified { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; } = "";

        [JsonPropertyName("data")]
        public string Data { get; set; } = "";

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("item_id")]
        public int ItemId { get; set; }

        [JsonPropertyName("user")]
        public UserDto? User { get; set; }

        public LogAkcji ToModel() => new()
        {
            Id = Id,
            Data = DateModified,
            DostepneDane = this.Data,
            ItemId = ItemId,
            Uzytkownik = User?.Name ?? (UserId > 0 ? $"user#{UserId}" : ""),
            Typ = Event switch
            {
                "user_login_success" => TypAkcji.Logowanie,
                "user_logout" => TypAkcji.Wylogowanie,
                var e when e.EndsWith("_create") || e.EndsWith("_add") => TypAkcji.Dodanie,
                var e when e.EndsWith("_update") || e.EndsWith("_edit") => TypAkcji.Edycja,
                var e when e.EndsWith("_delete") || e.EndsWith("_remove") => TypAkcji.Usuniecie,
                var e when e.Contains("export") => TypAkcji.Eksport,
                var e when e.Contains("import") => TypAkcji.Import,
                _ => TypAkcji.Inne
            },
            Opis = Event
        };
    }

    public class LogiResponse
    {
        [JsonPropertyName("items")]
        public List<LogAkcjiDto> Items { get; set; } = new();

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}