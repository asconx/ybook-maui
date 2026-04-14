using System.Text;
using System.Text.Json;
using yBook.Models;

namespace yBook.Services;

public interface IPanelService
{
    Task<List<Pokoj>> GetPokoje();
    Task<Pokoj> GetPokoj(int id);
    Task<Pokoj> CreatePokoj(Pokoj pokoj);
    Task<bool> UpdatePokoj(int id, Pokoj pokoj);
    Task<bool> DeletePokoj(int id);
}

public class PanelService : IPanelService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    // MAUI sam wstrzyknie HttpClient zarejestrowany w MauiProgram.cs
    public PanelService(IAuthService authService, HttpClient httpClient)
    {
        _authService = authService;
        _httpClient = httpClient;
    }

    // Primary endpoint - official API
    private readonly string _baseEndpoint = "https://api.ybook.pl/entity/room";

    // Fallback endpoints if primary fails
    private static readonly string[] FallbackEndpoints = new[]
    {
        "https://panel.ybook.pl/rooms",
        "https://panel.ybook.pl/api/rooms",
        "https://panel.ybook.pl/api/kwatery",
        "https://panel.ybook.pl/kwatery",
        "https://panel.ybook.pl/api/v1/rooms",
        "https://panel.ybook.pl/api/properties/rooms",
    };

  

    private async Task<string> GetAuthTokenAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Brak tokenu autoryzacyjnego");
        return token;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public async Task<List<Pokoj>> GetPokoje()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[PanelService] ==================== GET POKOJE START ====================");

            if (!await _authService.IsAuthenticatedAsync())
            {
                System.Diagnostics.Debug.WriteLine("[PanelService] ✗ User is not authenticated");
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");
            }

            System.Diagnostics.Debug.WriteLine("[PanelService] ✓ User authenticated");

            var token = await GetAuthTokenAsync();
            System.Diagnostics.Debug.WriteLine($"[PanelService] ✓ Token retrieved (length: {token.Length})");

            // Try primary endpoint first
            System.Diagnostics.Debug.WriteLine($"[PanelService] Attempting primary endpoint: {_baseEndpoint}");
            try
            {
                var request = CreateRequest(HttpMethod.Get, _baseEndpoint, token);
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[PanelService] Primary endpoint response status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[PanelService] Response size: {responseContent.Length} bytes");
                System.Diagnostics.Debug.WriteLine($"[PanelService] Response preview: {responseContent.Substring(0, Math.Min(300, responseContent.Length))}...");

                if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(responseContent))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    List<PanelRoom>? result = null;

                    System.Diagnostics.Debug.WriteLine("[PanelService] Parsing response...");

                    // API returns: { items: [...], total: N }
                    if (responseContent.StartsWith("{"))
                    {
                        System.Diagnostics.Debug.WriteLine("[PanelService] Response is a wrapped object, extracting data...");
                        var wrapper = JsonSerializer.Deserialize<JsonElement>(responseContent, options);

                        // Try items property first (most common)
                        if (wrapper.TryGetProperty("items", out var items) && items.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            System.Diagnostics.Debug.WriteLine("[PanelService] ✓ Found 'items' array");
                            result = JsonSerializer.Deserialize<List<PanelRoom>>(items.GetRawText(), options);
                        }
                        // Try data property as fallback
                        else if (wrapper.TryGetProperty("data", out var data) && data.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            System.Diagnostics.Debug.WriteLine("[PanelService] ✓ Found 'data' array");
                            result = JsonSerializer.Deserialize<List<PanelRoom>>(data.GetRawText(), options);
                        }

                        // Log total count if available
                        if (wrapper.TryGetProperty("total", out var total))
                        {
                            System.Diagnostics.Debug.WriteLine($"[PanelService] API reported total: {total.GetInt32()}");
                        }
                    }
                    else if (responseContent.StartsWith("["))
                    {
                        System.Diagnostics.Debug.WriteLine("[PanelService] Response is a direct array");
                        result = JsonSerializer.Deserialize<List<PanelRoom>>(responseContent, options);
                    }

                    if (result != null && result.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[PanelService] ✓ Successfully parsed {result.Count} rooms");
                        var mappedRooms = result.Select(MapToPiok).ToList();

                        // Log first few rooms
                        foreach (var room in mappedRooms.Take(5))
                        {
                            System.Diagnostics.Debug.WriteLine($"[PanelService]   - Room ID: {room.Id}, Name: {room.Nazwa}, Available: {room.CzyDostepny}, People: {room.MaxOsobLiczbą}");
                        }
                        if (mappedRooms.Count > 5)
                            System.Diagnostics.Debug.WriteLine($"[PanelService]   ... and {mappedRooms.Count - 5} more rooms");

                        System.Diagnostics.Debug.WriteLine("[PanelService] ==================== GET POKOJE COMPLETE (PRIMARY) ====================");
                        return mappedRooms;

                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PanelService] ✗ Primary endpoint failed: {ex.GetType().Name}: {ex.Message}");
            }

            // Try fallback endpoints
            System.Diagnostics.Debug.WriteLine("[PanelService] Trying fallback endpoints...");
            Exception lastException = null;
            int fallbackIndex = 0;
            foreach (var endpoint in FallbackEndpoints)
            {
                fallbackIndex++;
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[PanelService] [{fallbackIndex}/{FallbackEndpoints.Length}] Trying fallback: {endpoint}");
                    var request = CreateRequest(HttpMethod.Get, endpoint, token);
                    var response = await _httpClient.SendAsync(request);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"[PanelService] Fallback response status: {response.StatusCode}");

                    if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(responseContent))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        List<PanelRoom>? result = null;

                        // Try as list
                        if (responseContent.StartsWith("["))
                        {
                            System.Diagnostics.Debug.WriteLine("[PanelService] Parsing as array");
                            result = JsonSerializer.Deserialize<List<PanelRoom>>(responseContent, options);
                        }
                        else if (responseContent.StartsWith("{"))
                        {
                            System.Diagnostics.Debug.WriteLine("[PanelService] Parsing as wrapped object");
                            // Try wrapper properties (items first, then data, then rooms)
                            var wrapper = JsonSerializer.Deserialize<JsonElement>(responseContent, options);
                            if (wrapper.TryGetProperty("items", out var items))
                            {
                                System.Diagnostics.Debug.WriteLine("[PanelService] Found 'items' property");
                                result = JsonSerializer.Deserialize<List<PanelRoom>>(items.GetRawText(), options);
                            }
                            else if (wrapper.TryGetProperty("data", out var data))
                            {
                                System.Diagnostics.Debug.WriteLine("[PanelService] Found 'data' property");
                                result = JsonSerializer.Deserialize<List<PanelRoom>>(data.GetRawText(), options);
                            }
                            else if (wrapper.TryGetProperty("rooms", out var rooms))
                            {
                                System.Diagnostics.Debug.WriteLine("[PanelService] Found 'rooms' property");
                                result = JsonSerializer.Deserialize<List<PanelRoom>>(rooms.GetRawText(), options);
                            }
                        }

                        if (result != null && result.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[PanelService] ✓ Successfully parsed {result.Count} rooms from fallback endpoint");
                            var mappedRooms = result.Select(MapToPiok).ToList();
                            System.Diagnostics.Debug.WriteLine("[PanelService] ==================== GET POKOJE COMPLETE (FALLBACK) ====================");
                            return mappedRooms;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    System.Diagnostics.Debug.WriteLine($"[PanelService] ✗ Fallback {fallbackIndex} failed: {ex.GetType().Name}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[PanelService] ✗ All endpoints failed. Last error: {lastException?.Message}");
            System.Diagnostics.Debug.WriteLine("[PanelService] ==================== GET POKOJE FAILED ====================");
            throw new Exception($"Nie udało się połączyć z API. Ostatni błąd: {lastException?.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PanelService] ✗ CRITICAL ERROR: {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[PanelService] Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine("[PanelService] ==================== GET POKOJE FAILED (CRITICAL) ====================");
            throw new Exception($"Błąd pobierania pokojów: {ex.Message}", ex);
        }
    }

    public async Task<Pokoj> GetPokoj(int id)
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");

            var token = await GetAuthTokenAsync();
            var request = CreateRequest(HttpMethod.Get, $"{_baseEndpoint}/{id}", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Błąd serwera: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var panelRoom = JsonSerializer.Deserialize<PanelRoom>(json, options);

            return panelRoom != null ? MapToPiok(panelRoom) : null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Błąd pobierania pokoju: {ex.Message}", ex);
        }
    }

    public async Task<Pokoj> CreatePokoj(Pokoj pokoj)
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");

            var token = await GetAuthTokenAsync();
            var request = CreateRequest(HttpMethod.Post, _baseEndpoint, token);

            var panelRoom = MapFromPokoj(pokoj);
            var json = JsonSerializer.Serialize(panelRoom);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Błąd serwera: {response.StatusCode}");

            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var createdRoom = JsonSerializer.Deserialize<PanelRoom>(responseJson, options);

            return createdRoom != null ? MapToPiok(createdRoom) : null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Błąd dodawania pokoju: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdatePokoj(int id, Pokoj pokoj)
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");

            var token = await GetAuthTokenAsync();
            var request = CreateRequest(HttpMethod.Put, $"{_baseEndpoint}/{id}", token);

            var panelRoom = MapFromPokoj(pokoj);
            var json = JsonSerializer.Serialize(panelRoom);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new Exception($"Błąd aktualizacji pokoju: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeletePokoj(int id)
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");

            var token = await GetAuthTokenAsync();
            var request = CreateRequest(HttpMethod.Delete, $"{_baseEndpoint}/{id}", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new Exception($"Błąd usuwania pokoju: {ex.Message}", ex);
        }
    }

    // Mapowanie z panelu na model
    private Pokoj MapToPiok(PanelRoom panelRoom)
    {
        return new Pokoj
        {
            Id = panelRoom.Id,
            OrganizationId = panelRoom.OrganizationId ?? 0,
            PropertyId = panelRoom.PropertyId ?? 0,
            DateModified = panelRoom.DateModified ?? DateTime.Now.ToString(),
            Nazwa = panelRoom.Name ?? string.Empty,
            Type = panelRoom.Type ?? 0,
            CzyDostepny = panelRoom.IsAvailable == 1,
            MaxOsobLiczbą = panelRoom.MaxNumberOfPeople ?? 0,
            Powierzchnia = panelRoom.Area?.ToString() ?? "0",
            Opis = panelRoom.Description ?? string.Empty,
            ShortName = panelRoom.ShortName ?? string.Empty,
            DefaultPrice = panelRoom.DefaultPrice ?? 0,
            Kolor = panelRoom.Color ?? "#ffffff",
            Standard = panelRoom.Standard ?? string.Empty,
            MinOsobLiczbą = panelRoom.MinNumberOfPeople ?? 0,
            LockId = panelRoom.LockId ?? 0,
            CalendarPosition = panelRoom.CalendarPosition ?? 0
        };
    }

    // Mapowanie z modelu na panel
    private PanelRoom MapFromPokoj(Pokoj pokoj)
    {
        return new PanelRoom
        {
            Id = pokoj.Id,
            OrganizationId = pokoj.OrganizationId,
            PropertyId = pokoj.PropertyId,
            DateModified = pokoj.DateModified,
            Name = pokoj.Nazwa,
            Type = pokoj.Type,
            IsAvailable = pokoj.CzyDostepny ? 1 : 0,
            MaxNumberOfPeople = pokoj.MaxOsobLiczbą,
            Area = int.TryParse(pokoj.Powierzchnia, out var area) ? area : 0,
            Description = pokoj.Opis,
            ShortName = pokoj.ShortName,
            DefaultPrice = pokoj.DefaultPrice,
            Color = pokoj.Kolor,
            Standard = pokoj.Standard,
            MinNumberOfPeople = pokoj.MinOsobLiczbą,
            LockId = pokoj.LockId,
            CalendarPosition = pokoj.CalendarPosition
        };
    }
}

// Modele dla API api.ybook.pl/entity/room
public class PanelRoom
{
    public int Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("organization_id")]
    public int? OrganizationId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("property_id")]
    public int? PropertyId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("date_modified")]
    public string? DateModified { get; set; }

    public string? Name { get; set; }

    public int? Type { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("is_available")]
    public int IsAvailable { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("max_number_of_people")]
    public int? MaxNumberOfPeople { get; set; }

    // Area może być string lub int - musimy obsługiwać oba
    [System.Text.Json.Serialization.JsonConverter(typeof(StringToIntConverter))]
    public int? Area { get; set; }

    public string? Description { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("short_name")]
    public string? ShortName { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("default_price")]
    public int? DefaultPrice { get; set; }

    public string? Color { get; set; }

    public string? Standard { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("min_number_of_people")]
    public int? MinNumberOfPeople { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("lock_id")]
    public int? LockId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("calendar_position")]
    public int? CalendarPosition { get; set; }
}

// Custom converter dla pól które mogą być string lub int
public class StringToIntConverter : System.Text.Json.Serialization.JsonConverter<int?>
{
    public override int? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        try
        {
            switch (reader.TokenType)
            {
                case System.Text.Json.JsonTokenType.Number:
                    return reader.GetInt32();
                case System.Text.Json.JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrEmpty(stringValue))
                        return null;
                    if (int.TryParse(stringValue, out var result))
                        return result;
                    return null;
                case System.Text.Json.JsonTokenType.Null:
                    return null;
                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, int? value, System.Text.Json.JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}
