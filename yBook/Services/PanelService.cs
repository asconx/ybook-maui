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

    // Spróbuj te endpointy w kolejności
    private static readonly string[] PossibleEndpoints = new[]
    {
        "https://panel.ybook.pl/rooms",            // ✅ PRAWIDŁOWY!
        "https://panel.ybook.pl/api/rooms",
        "https://panel.ybook.pl/api/kwatery",
        "https://panel.ybook.pl/kwatery",
        "https://panel.ybook.pl/api/v1/rooms",
        "https://panel.ybook.pl/api/properties/rooms",
        "https://api.ybook.pl/entity/room",        // Powrót do starego
    };

    private string _currentEndpoint = "https://panel.ybook.pl/api/rooms";

    public PanelService(IAuthService authService)
    {
        _authService = authService;
        _httpClient = new HttpClient();
    }

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
            if (!await _authService.IsAuthenticatedAsync())
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");

            var token = await GetAuthTokenAsync();

            // Spróbuj każdy endpoint
            Exception lastException = null;
            foreach (var endpoint in PossibleEndpoints)
            {
                try
                {
                    _currentEndpoint = endpoint;
                    var request = CreateRequest(HttpMethod.Get, endpoint, token);
                    var response = await _httpClient.SendAsync(request);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[PanelService] Endpoint: {endpoint}");
                    System.Diagnostics.Debug.WriteLine($"[PanelService] Status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[PanelService] Response: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}");

                    if (response.IsSuccessStatusCode && responseContent.StartsWith("[") || responseContent.StartsWith("{"))
                    {
                        // Udało się!
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        List<PanelRoom> result = null;

                        // Spróbuj jako lista
                        if (responseContent.StartsWith("["))
                        {
                            result = JsonSerializer.Deserialize<List<PanelRoom>>(responseContent, options);
                        }
                        else if (responseContent.StartsWith("{"))
                        {
                            // Spróbuj jako wrapper z "data", "items", "rooms"
                            var wrapper = JsonSerializer.Deserialize<JsonElement>(responseContent, options);
                            if (wrapper.TryGetProperty("data", out var data))
                                result = JsonSerializer.Deserialize<List<PanelRoom>>(data.GetRawText(), options);
                            else if (wrapper.TryGetProperty("items", out var items))
                                result = JsonSerializer.Deserialize<List<PanelRoom>>(items.GetRawText(), options);
                            else if (wrapper.TryGetProperty("rooms", out var rooms))
                                result = JsonSerializer.Deserialize<List<PanelRoom>>(rooms.GetRawText(), options);
                        }

                        if (result != null)
                            return result.Select(MapToPiok).ToList();
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    System.Diagnostics.Debug.WriteLine($"[PanelService] Failed endpoint {endpoint}: {ex.Message}");
                    continue;
                }
            }

            throw new Exception($"Nie udało się połączyć z API. Ostatni błąd: {lastException?.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PanelService] GetPokoje Error: {ex}");
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
            var request = CreateRequest(HttpMethod.Get, $"{_currentEndpoint}/{id}", token);

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
            var request = CreateRequest(HttpMethod.Post, _currentEndpoint, token);

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
            var request = CreateRequest(HttpMethod.Put, $"{_currentEndpoint}/{id}", token);

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
            var request = CreateRequest(HttpMethod.Delete, $"{_currentEndpoint}/{id}", token);

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

// Modele dla API panel.ybook.pl
public class PanelRoom
{
    public int Id { get; set; }
    public int? OrganizationId { get; set; }
    public int? PropertyId { get; set; }
    public string DateModified { get; set; }
    public string Name { get; set; }
    public int? Type { get; set; }
    public int IsAvailable { get; set; }
    public int? MaxNumberOfPeople { get; set; }

    // Area może być string lub int - musimy obsługiwać oba
    [System.Text.Json.Serialization.JsonConverter(typeof(StringToIntConverter))]
    public int? Area { get; set; }

    public string Description { get; set; }
    public string ShortName { get; set; }
    public int? DefaultPrice { get; set; }
    public string Color { get; set; }
    public string Standard { get; set; }
    public int? MinNumberOfPeople { get; set; }
    public int? LockId { get; set; }
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
