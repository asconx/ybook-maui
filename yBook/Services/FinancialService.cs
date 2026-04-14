using System.Text.Json;
using System.Text;
using Microsoft.Maui.Storage;
using yBook.Models;

namespace yBook.Services;

public class FinancialService
{
    private readonly HttpClient _http;

    public FinancialService(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        if (_http.BaseAddress == null)
            _http.BaseAddress = new Uri("https://api.ybook.pl/");
    }

    private async Task AddJwt()
    {
        var token = await SecureStorage.GetAsync("jwt_token");
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<FinancialListResponse?> GetByServiceId(int serviceId, int start = 0, string startDate = "", string endDate = "")
    {
        await AddJwt();
        var url = $"financialDocuments/{serviceId}/listByServiceId?start={start}&startDate={startDate}&endDate={endDate}";
        var res = await _http.GetAsync(url);
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<FinancialListResponse>(json, options);
    }

    public async Task<bool> CreateDocument(FinancialCreateDto dto)
    {
        await AddJwt();
        // Endpoint tworzenia dokumentu: POST /financialDocuments/{serviceId}/create
        var url = $"financialDocuments/{dto.ServiceId}/create";
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        System.Diagnostics.Debug.WriteLine($"[FinancialService] POST {url} -> Request: {json}");
        var res = await _http.PostAsync(url, content);
        var body = await res.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"[FinancialService] CreateDocument -> {res.StatusCode}: {body}");
        return res.IsSuccessStatusCode;
    }
}
