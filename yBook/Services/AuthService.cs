using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace yBook.Services
{
    public class AuthService : IAuthService
    {
        private const string TokenKey  = "auth_token";
        private const string SessionKey = "auth_session";
        private const string BaseUrl   = "https://api.ybook.pl";

        private readonly HttpClient _http;
        private UserSession? _currentUser;

        public UserSession? CurrentUser => _currentUser;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        // ── Login ──────────────────────────────────────────────────────────────

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    email = email,
                    pass  = password
                });

                var content  = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync($"{BaseUrl}/authorize", content);

                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var dto  = JsonSerializer.Deserialize<AuthResponseDto>(json,
                               new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dto?.Token is null)
                    return false;

                // Zapisz token do SecureStorage
                await SecureStorage.Default.SetAsync(TokenKey, dto.Token);
                await SecureStorage.Default.SetAsync(SessionKey, json);

                // Ustaw sesję w pamięci
                _currentUser = new UserSession
                {
                    Token          = dto.Token,
                    OrganizationId = dto.OrganizationId,
                    Role           = dto.Role ?? string.Empty,
                    Permissions    = dto.Permissions ?? []
                };

                // Ustaw domyślny Authorization header dla wszystkich przyszłych requestów
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dto.Token);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] LoginAsync error: {ex.Message}");
                return false;
            }
        }

        // ── Restore session on app start ───────────────────────────────────────

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync(TokenKey);
                if (string.IsNullOrEmpty(token))
                    return false;

                // Sprawdź czy token JWT nie wygasł (dekodowanie bez weryfikacji sygnatury)
                if (IsTokenExpired(token))
                {
                    await LogoutAsync();
                    return false;
                }

                // Odtwórz sesję z SecureStorage
                var sessionJson = await SecureStorage.Default.GetAsync(SessionKey);
                if (sessionJson is not null)
                {
                    var dto = JsonSerializer.Deserialize<AuthResponseDto>(sessionJson,
                                  new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto is not null)
                    {
                        _currentUser = new UserSession
                        {
                            Token          = token,
                            OrganizationId = dto.OrganizationId,
                            Role           = dto.Role ?? string.Empty,
                            Permissions    = dto.Permissions ?? []
                        };
                    }
                }

                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── Token ──────────────────────────────────────────────────────────────

        public async Task<string?> GetTokenAsync()
            => await SecureStorage.Default.GetAsync(TokenKey);

        // ── Logout ─────────────────────────────────────────────────────────────

        public async Task LogoutAsync()
        {
            _currentUser = null;
            _http.DefaultRequestHeaders.Authorization = null;

            SecureStorage.Default.Remove(TokenKey);
            SecureStorage.Default.Remove(SessionKey);

            await Task.CompletedTask;
        }

        // ── JWT expiry check (bez zewnętrznych bibliotek) ──────────────────────

        private static bool IsTokenExpired(string token)
        {
            try
            {
                var parts   = token.Split('.');
                if (parts.Length != 3) return true;

                var payload = parts[1];
                // Base64url → Base64
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "=";  break;
                }

                var bytes = Convert.FromBase64String(payload);
                var json  = Encoding.UTF8.GetString(bytes);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("exp", out var expProp))
                {
                    var exp     = expProp.GetInt64();
                    var expDate = DateTimeOffset.FromUnixTimeSeconds(exp);
                    return expDate <= DateTimeOffset.UtcNow;
                }

                return false; // brak "exp" → traktuj jako ważny
            }
            catch
            {
                return true;
            }
        }

        // ── DTO ───────────────────────────────────────────────────────────────

        private class AuthResponseDto
        {
            [JsonPropertyName("token")]
            public string? Token { get; set; }

            [JsonPropertyName("organization_id")]
            public int OrganizationId { get; set; }

            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("permissions")]
            public List<string>? Permissions { get; set; }
        }
    }
}
