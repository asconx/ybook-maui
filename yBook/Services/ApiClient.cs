using System.Net.Http.Json;
using System.Diagnostics;
using System.Text.Json;

namespace yBook.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private const string BaseUrl = "https://api.ybook.pl";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(IAuthService authService = null)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _authService = authService;
        }

        private async Task EnsureAuthHeaderAsync()
        {
            if (_authService == null) return;

            try
            {
                var token = await _authService.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Debug.WriteLine($"[ApiClient] Authorization header set with token");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiClient] Error setting auth header: {ex.Message}");
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                await EnsureAuthHeaderAsync();

                var url = $"{BaseUrl}{endpoint}";
                Debug.WriteLine($"[ApiClient] GET: {url}");

                var response = await _httpClient.GetAsync(url);
                Debug.WriteLine($"[ApiClient] Status: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiClient] Response length: {json.Length} chars");
                Debug.WriteLine($"[ApiClient] Response (first 500 chars): {json.Substring(0, Math.Min(500, json.Length))}");

                var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
                Debug.WriteLine($"[ApiClient] Deserialized successfully");
                return result;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiClient] HTTP Error: {ex.Message}");
                Debug.WriteLine($"[ApiClient] Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"[ApiClient] JSON Deserialization Error: {ex.Message}");
                Debug.WriteLine($"[ApiClient] Line: {ex.LineNumber}, BytePositionInLine: {ex.BytePositionInLine}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiClient] Unexpected Error: {ex.GetType().Name} - {ex.Message}");
                Debug.WriteLine($"[ApiClient] Stack: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                await EnsureAuthHeaderAsync();

                var url = $"{BaseUrl}{endpoint}";
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseJson, JsonOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiClient] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                await EnsureAuthHeaderAsync();

                var url = $"{BaseUrl}{endpoint}";
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseJson, JsonOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiClient] Error: {ex.Message}");
                throw;
            }
        }
    }
}