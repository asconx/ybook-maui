using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace yBook.Services;

public class SmsService : ISmsService
{
    private readonly HttpClient _http;
    private readonly IAuthService _auth;
    private const string BaseUrl = "https://api.ybook.pl";

    public SmsService(IAuthService authService, HttpClient http)
    {
        _auth = authService;
        _http = http ?? new HttpClient();
    }

    public async Task<List<StatusDto>> GetStatusesAsync()
    {
        // Do not require authentication for reading statuses — API may allow public reads.
        var token = await _auth.GetTokenAsync();
        var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/entity/status");
        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await _http.SendAsync(req);
        System.Diagnostics.Debug.WriteLine($"[SmsService] GET {req.RequestUri} -> {(int)resp.StatusCode} {resp.ReasonPhrase}");
        if (!resp.IsSuccessStatusCode)
        {
            System.Diagnostics.Debug.WriteLine($"[SmsService] GetStatusesAsync failed: {(int)resp.StatusCode}");
            return new List<StatusDto>();
        }

        var json = await resp.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"[SmsService] GetStatusesAsync response length={json?.Length}");
        System.Diagnostics.Debug.WriteLine(json?.Length > 0 ? json.Substring(0, Math.Min(1000, json.Length)) : "<empty>");
        try
        {
            using var doc = JsonDocument.Parse(json);
            var list = new List<StatusDto>();
            // Handle several response shapes: array, { items: [...] }, { data: [...] }
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                    AddElementToList(el, list);
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in items.EnumerateArray())
                        AddElementToList(el, list);
                }
                else if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in data.EnumerateArray())
                        AddElementToList(el, list);
                }
            }

            static void AddElementToList(JsonElement el, List<StatusDto> list)
            {
                var dto = new StatusDto();
                if (el.ValueKind != JsonValueKind.Object) return;
                if (el.TryGetProperty("id", out var idProp) && idProp.TryGetInt32(out var id)) dto.Id = id;
                if (el.TryGetProperty("key", out var keyProp) && keyProp.ValueKind == JsonValueKind.String) dto.Key = keyProp.GetString();
                if (el.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String) dto.Name = nameProp.GetString();
                if (el.TryGetProperty("color", out var colorProp) && colorProp.ValueKind == JsonValueKind.String) dto.Color = colorProp.GetString();
                // sometimes name is under 'title' or 'label'
                if (string.IsNullOrEmpty(dto.Name))
                {
                    if (el.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String) dto.Name = t.GetString();
                    else if (el.TryGetProperty("label", out var l) && l.ValueKind == JsonValueKind.String) dto.Name = l.GetString();
                }
                list.Add(dto);
            }

            return list;
        }
        catch
        {
            return new List<StatusDto>();
        }
    }

    public async Task<bool> SendToActiveAsync(string message, IEnumerable<int> statusIds, bool checkedIn, bool notCheckedIn)
    {
        if (!await _auth.IsAuthenticatedAsync())
            return false;

        var token = await _auth.GetTokenAsync();
        var url = $"{BaseUrl}/multiSms/sendToActive";
        var payload = new
        {
            checkedIn = checkedIn,
            notCheckedIn = notCheckedIn,
            statusIds = statusIds.ToArray(),
            message = message
        };

        var json = JsonSerializer.Serialize(payload);
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<int> GetActiveCountAsync(IEnumerable<int> statusIds, bool checkedIn, bool notCheckedIn)
    {
        if (!await _auth.IsAuthenticatedAsync())
            return 0;

        var token = await _auth.GetTokenAsync();
        var url = $"{BaseUrl}/multiSms/countActive"; // current API should provide such endpoint

        var payload = new
        {
            checkedIn = checkedIn,
            notCheckedIn = notCheckedIn,
            statusIds = statusIds.ToArray()
        };

        var json = JsonSerializer.Serialize(payload);
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return 0;

        var respJson = await resp.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(respJson);
            // common shape: { "count": 123 }
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("count", out var cnt) && cnt.TryGetInt32(out var v))
                return v;

            // or direct number
            if (doc.RootElement.ValueKind == JsonValueKind.Number && doc.RootElement.TryGetInt32(out var direct))
                return direct;

            // or wrapper like { data: { count: 123 } }
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
            {
                if (data.TryGetProperty("count", out var c2) && c2.TryGetInt32(out var v2)) return v2;
            }
        }
        catch { }

        return 0;
    }
}
