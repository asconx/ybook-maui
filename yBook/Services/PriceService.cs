using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Models;

namespace yBook.Services;

// ─── DTO Classes for API Response ────────────────────────────────────────

public class ApiPriceModifierItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("organization_id")]
    public int? OrganizationId { get; set; }

    [JsonPropertyName("date_modified")]
    public string? DateModified { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("is_mandatory")]
    public int? IsMandatory { get; set; }

    [JsonPropertyName("is_service")]
    public int? IsService { get; set; }

    [JsonPropertyName("is_percent_amount")]
    public int? IsPercentAmount { get; set; }

    [JsonPropertyName("is_per_person")]
    public int? IsPerPerson { get; set; }

    [JsonPropertyName("is_daily")]
    public int? IsDaily { get; set; }

    [JsonPropertyName("is_for_room")]
    public int? IsForRoom { get; set; }

    [JsonPropertyName("is_for_period")]
    public int? IsForPeriod { get; set; }

    [JsonPropertyName("is_per_person_type")]
    public int? IsPerPersonType { get; set; }

    [JsonPropertyName("amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("amount_adult")]
    public string? AmountAdult { get; set; }

    [JsonPropertyName("amount_kid")]
    public string? AmountKid { get; set; }

    [JsonPropertyName("amount_infant")]
    public string? AmountInfant { get; set; }

    [JsonPropertyName("apply_from_date")]
    public string? ApplyFromDate { get; set; }

    [JsonPropertyName("apply_to_date")]
    public string? ApplyToDate { get; set; }

    [JsonPropertyName("min_days")]
    public int? MinDays { get; set; }

    [JsonPropertyName("max_days")]
    public int? MaxDays { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("is_monday")]
    public int? IsMonday { get; set; }

    [JsonPropertyName("is_tuesday")]
    public int? IsTuesday { get; set; }

    [JsonPropertyName("is_wednesday")]
    public int? IsWednesday { get; set; }

    [JsonPropertyName("is_thursday")]
    public int? IsThursday { get; set; }

    [JsonPropertyName("is_friday")]
    public int? IsFriday { get; set; }

    [JsonPropertyName("is_saturday")]
    public int? IsSaturday { get; set; }

    [JsonPropertyName("is_sunday")]
    public int? IsSunday { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("is_fiscal")]
    public int? IsFiscal { get; set; }

    [JsonPropertyName("amount_age_group_1")]
    public string? AmountAgeGroup1 { get; set; }

    [JsonPropertyName("amount_age_group_2")]
    public string? AmountAgeGroup2 { get; set; }

    [JsonPropertyName("amount_age_group_3")]
    public string? AmountAgeGroup3 { get; set; }

    [JsonPropertyName("amount_age_group_4")]
    public string? AmountAgeGroup4 { get; set; }

    [JsonPropertyName("amount_age_group_5")]
    public string? AmountAgeGroup5 { get; set; }

    [JsonPropertyName("photo_file_id")]
    public int? PhotoFileId { get; set; }

    [JsonPropertyName("include_discount")]
    public int? IncludeDiscount { get; set; }

    [JsonPropertyName("has_selected_rooms")]
    public int? HasSelectedRooms { get; set; }

    [JsonPropertyName("is_cost")]
    public int? IsCost { get; set; }
}

public class PriceResponseDto
{
    [JsonPropertyName("items")]
    public List<ApiPriceModifierItem>? Items { get; set; }
}

// ─── PriceService Implementation ──────────────────────────────────────────

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
                return new();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<PriceResponseDto>(json, options);

            if (dto?.Items == null || dto.Items.Count == 0)
                return new();

            var result = new List<CennikItem>();
            foreach (var item in dto.Items)
            {
                var cennikItem = MapApiToCennikItem(item);
                if (cennikItem != null)
                    result.Add(cennikItem);
            }

            System.Diagnostics.Debug.WriteLine($"[PriceService] Fetched {result.Count} price modifiers from API");
            return result;
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PriceService] HTTP error: {httpEx.Message}");
            return new();
        }
        catch (TaskCanceledException timeoutEx)
        {
            System.Diagnostics.Debug.WriteLine($"[PriceService] Timeout: {timeoutEx.Message}");
            return new();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PriceService] FetchPriceModifiersAsync error: {ex.Message}");
            return new();
        }
    }

    private CennikItem? MapApiToCennikItem(ApiPriceModifierItem apiItem)
    {
        try
        {
            if (string.IsNullOrEmpty(apiItem.Name))
                return null;

            var id = apiItem.Id.ToString();
            var nazwa = apiItem.Name;

            // Parse amount - prefer amount if available, fallback to amount_adult
            var cena = ParseDecimal(apiItem.Amount) ?? ParseDecimal(apiItem.AmountAdult) ?? 0m;

            // Parse dates
            var od = ParseDateTime(apiItem.ApplyFromDate) ?? DateTime.Today;
            var do_data = ParseDateTime(apiItem.ApplyToDate) ?? DateTime.Today.AddDays(1);

            var priorytet = apiItem.Priority ?? 1;

            // Pokoje: API doesn't provide rooms array directly, so default to empty list
            // The user can assign rooms through a separate interface if needed
            var pokoje = new List<string>();

            return new CennikItem
            {
                Id = id,
                Nazwa = nazwa,
                Cena = cena,
                Priorytet = priorytet,
                Od = od,
                Do = do_data,
                Pokoje = pokoje
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PriceService] MapApiToCennikItem error: {ex.Message}");
            return null;
        }
    }

    private decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (decimal.TryParse(value, out var result))
            return result;

        return null;
    }

    private DateTime? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTime.TryParse(value, out var result))
            return result;

        return null;
    }
}
