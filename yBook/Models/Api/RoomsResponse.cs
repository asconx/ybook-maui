using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace yBook.Models.Api
{
    /// <summary>
    /// Model odpowiedzi API zawierający listę elementów i sumaryczną wartość total
    /// </summary>
    public class RoomsResponse
    {
        [JsonPropertyName("items")]
        public List<Room> Items { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
