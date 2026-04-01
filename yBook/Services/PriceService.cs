using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Models;

namespace yBook.Services;

public class PriceService : IPriceService
{
    private const string BaseUrl = "https://api.ybook.pl";
    private readonly HttpClient _http;

    public PriceService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CennikItem>> FetchPriceModifiersAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/entity/priceModifier");

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = response.StatusCode;
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[PriceService] API error {statusCode}: {content}");
                return new List<CennikItem>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // The API returns items in a wrapper similar to survey. Try to deserialize to DTO first
            var dto = JsonSerializer.Deserialize<PriceResponseDto>(json, options);
            List<CennikItem> items = new();

            if (dto?.Items != null)
            {
                // Map API objects to CennikItem. Adjust property names if API differs.
                items.AddRange(dto.Items.Select(i => new CennikItem
                {
                    Id = i.Id?.ToString() ?? string.Empty,
                    Nazwa = i.Name ?? i.Title ?? "",
                    Cena = i.Price ?? 0m,
                    Priorytet = i.Priority ?? 1,
                    Od = i.DateFrom ?? DateTime.Today,
                    Do = i.DateTo ?? DateTime.Today,
                    Pokoje = i.Rooms ?? new List<string>()
                }));
            }
            else
            {
                // Fallback: try deserialize directly to CennikItem list
                var list = JsonSerializer.Deserialize<List<CennikItem>>(json, options);
                if (list != null) items.AddRange(list);
            }

            System.Diagnostics.Debug.WriteLine($"[PriceService] Pobrano {items.Count} cenników z API");
            return items;
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PriceService] HTTP error: {httpEx.Message}");
            return new List<CennikItem>();
        }
        catch (TaskCanceledException timeoutEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PriceService] Timeout: {timeoutEx.Message}");
            return new List<CennikItem>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PriceService] FetchPriceModifiersAsync error: {ex.Message}");
            return new List<CennikItem>();
        }
    }

    private class PriceResponseDto
    {
        [JsonPropertyName("items")]
        public List<ApiPriceItem> Items { get; set; } = new();
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    private class ApiPriceItem
    {
        [JsonPropertyName("id")] public JsonElement? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("price")] public decimal? Price { get; set; }
        [JsonPropertyName("priority")] public int? Priority { get; set; }
        [JsonPropertyName("dateFrom")] public DateTime? DateFrom { get; set; }
        [JsonPropertyName("dateTo")] public DateTime? DateTo { get; set; }
        [JsonPropertyName("rooms")] public List<string>? Rooms { get; set; }
    }
}
