using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using yBook.Models;

namespace yBook.Services
{
    /// <summary>
    /// Serwis finansowy łączący się z API yBook (panel.ybook.pl).
    /// Endpointy bazują na wzorcu z PanelService — Bearer token z IAuthService.
    /// </summary>
    public class FinanseService : IFinanseService
    {
        private const string BaseUrl = "https://api.ybook.pl";

        private readonly HttpClient   _http;
        private readonly IAuthService _auth;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public FinanseService(IAuthService auth)
        {
            _auth = auth;
            _http = new HttpClient();
        }

        // ── Pomocnicze ────────────────────────────────────────────────────────

        private async Task<HttpRequestMessage> BuildRequest(HttpMethod method, string url, object? body = null)
        {
            var token = await _auth.GetTokenAsync()
                        ?? throw new UnauthorizedAccessException("Brak tokenu autoryzacyjnego.");

            var req = new HttpRequestMessage(method, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body is not null)
                req.Content = new StringContent(
                    JsonSerializer.Serialize(body, JsonOpts),
                    Encoding.UTF8,
                    "application/json");

            return req;
        }

        private static List<T> ParseList<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];

            if (json.TrimStart().StartsWith('['))
                return JsonSerializer.Deserialize<List<T>>(json, JsonOpts) ?? [];

            // Wrapper z kluczem "data" / "items"
            var el = JsonSerializer.Deserialize<JsonElement>(json, JsonOpts);
            foreach (var key in new[] { "data", "items", "results", "payments", "documents", "accounts" })
                if (el.TryGetProperty(key, out var arr))
                    return JsonSerializer.Deserialize<List<T>>(arr.GetRawText(), JsonOpts) ?? [];

            return [];
        }

        // ── Rejestr płatności ─────────────────────────────────────────────────

        public async Task<List<Platnosc>> GetPlatnosciAsync(
            DateTime? od = null, DateTime? do_ = null, string? kontoId = null)
        {
            try
            {
                var orgId = _auth.CurrentUser?.OrganizationId;
                var qs    = BuildQuery(od, do_, kontoId is not null ? ("account", kontoId) : default);
                var url   = orgId.HasValue
                    ? $"{BaseUrl}/organizations/{orgId}/payments{qs}"
                    : $"{BaseUrl}/payments{qs}";

                var req  = await BuildRequest(HttpMethod.Get, url);
                var resp = await _http.SendAsync(req);

                if (!resp.IsSuccessStatusCode) return MockFinanse.Platnosci(); // fallback

                var json = await resp.Content.ReadAsStringAsync();
                var raw  = ParseList<ApiPlatnosc>(json);
                return raw.Select(MapPlatnosc).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FinanseService.GetPlatnosci] {ex.Message}");
                return MockFinanse.Platnosci(); // fallback na mock gdy API niedostępne
            }
        }

        public async Task<Platnosc> CreatePlatnoscAsync(Platnosc p)
        {
            var orgId = _auth.CurrentUser?.OrganizationId;
            var url   = orgId.HasValue
                ? $"{BaseUrl}/organizations/{orgId}/payments"
                : $"{BaseUrl}/payments";

            var body = new
            {
                title          = p.Tytul,
                type           = p.Typ.ToString().ToLower(),
                client         = p.Klient,
                reservationNo  = p.NumerRezerwacji,
                date           = p.Data.ToString("yyyy-MM-dd"),
                amount         = p.Kwota,
                income         = p.Przychod
            };

            var req  = await BuildRequest(HttpMethod.Post, url, body);
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var json   = await resp.Content.ReadAsStringAsync();
            var apiRec = JsonSerializer.Deserialize<ApiPlatnosc>(json, JsonOpts)!;
            return MapPlatnosc(apiRec);
        }

        // ── Dokumenty ─────────────────────────────────────────────────────────

        public async Task<List<Dokument>> GetDokumentyAsync(
            DateTime? od = null, DateTime? do_ = null, string? typ = null, string? klient = null)
        {
            try
            {
                var orgId = _auth.CurrentUser?.OrganizationId;
                var qs    = BuildQuery(od, do_, typ is not null ? ("type", typ) : default,
                                               klient is not null ? ("client", klient) : default);
                var url   = orgId.HasValue
                    ? $"{BaseUrl}/organizations/{orgId}/documents{qs}"
                    : $"{BaseUrl}/documents{qs}";

                var req  = await BuildRequest(HttpMethod.Get, url);
                var resp = await _http.SendAsync(req);

                if (!resp.IsSuccessStatusCode) return MockFinanse.Dokumenty();

                var json = await resp.Content.ReadAsStringAsync();
                var raw  = ParseList<ApiDokument>(json);
                return raw.Select(MapDokument).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FinanseService.GetDokumenty] {ex.Message}");
                return MockFinanse.Dokumenty();
            }
        }

        public async Task<Dokument> CreateDokumentAsync(Dokument d)
        {
            var orgId = _auth.CurrentUser?.OrganizationId;
            var url   = orgId.HasValue
                ? $"{BaseUrl}/organizations/{orgId}/documents"
                : $"{BaseUrl}/documents";

            var body = new
            {
                reservationNo   = d.NumerRezerwacji,
                number          = d.Numer,
                type            = d.Typ.ToString().ToLower(),
                client          = d.Klient,
                grossAmount     = d.KwotaBrutto,
                paidAmount      = d.KwotaWplaty,
                issueDate       = d.DataWystawienia.ToString("yyyy-MM-dd"),
                saleDate        = d.DataSprzedazy?.ToString("yyyy-MM-dd"),
                paymentDate     = d.DataPlatnosci?.ToString("yyyy-MM-dd")
            };

            var req  = await BuildRequest(HttpMethod.Post, url, body);
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var json   = await resp.Content.ReadAsStringAsync();
            var apiRec = JsonSerializer.Deserialize<ApiDokument>(json, JsonOpts)!;
            return MapDokument(apiRec);
        }

        // ── Konta finansowe ───────────────────────────────────────────────────

        public async Task<List<KontoFinansowe>> GetKontaAsync()
        {
            try
            {
                var url  = $"{BaseUrl}/entity/account";
                var req  = await BuildRequest(HttpMethod.Get, url);
                var resp = await _http.SendAsync(req);

                if (!resp.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[FinanseService.GetKonta] HTTP {resp.StatusCode}");
                    return MockFinanse.Konta();
                }

                var json = await resp.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[FinanseService.GetKonta] Response: {json.Substring(0, Math.Min(300, json.Length))}");
                var raw  = ParseList<ApiKonto>(json);
                return raw.Select(MapKonto).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FinanseService.GetKonta] {ex.Message}");
                return MockFinanse.Konta();
            }
        }

        public async Task<KontoFinansowe> CreateKontoAsync(KontoFinansowe k)
        {
            var url = $"{BaseUrl}/entity/account";

            var body = new
            {
                name     = k.Nazwa,
                type     = k.Typ.ToString().ToLower(),
                number   = k.Numer,
                balance  = k.Saldo,
                currency = k.Waluta
            };

            var req  = await BuildRequest(HttpMethod.Post, url, body);
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var json   = await resp.Content.ReadAsStringAsync();
            var apiRec = JsonSerializer.Deserialize<ApiKonto>(json, JsonOpts)!;
            return MapKonto(apiRec);
        }

        public async Task<bool> DeleteKontoAsync(string id)
        {
            try
            {
                var url = $"{BaseUrl}/entity/account/{id}";

                var req  = await BuildRequest(HttpMethod.Delete, url);
                var resp = await _http.SendAsync(req);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── Import MT940 ──────────────────────────────────────────────────────

        public async Task<bool> ImportMT940Async(Stream fileStream, string fileName)
        {
            try
            {
                var token = await _auth.GetTokenAsync()
                            ?? throw new UnauthorizedAccessException("Brak tokenu.");

                var orgId = _auth.CurrentUser?.OrganizationId;
                var url   = orgId.HasValue
                    ? $"{BaseUrl}/organizations/{orgId}/bank-imports/mt940"
                    : $"{BaseUrl}/bank-imports/mt940";

                using var content     = new MultipartFormDataContent();
                using var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, "file", fileName);

                var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await _http.SendAsync(req);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FinanseService.ImportMT940] {ex.Message}");
                return false;
            }
        }

        // ── Query builder ─────────────────────────────────────────────────────

        private static string BuildQuery(DateTime? od, DateTime? do_,
            params (string key, string val)[] extras)
        {
            var parts = new List<string>();
            if (od.HasValue)  parts.Add($"dateFrom={od.Value:yyyy-MM-dd}");
            if (do_.HasValue) parts.Add($"dateTo={do_.Value:yyyy-MM-dd}");
            foreach (var (k, v) in extras)
                if (k is not null && v is not null) parts.Add($"{k}={Uri.EscapeDataString(v)}");
            return parts.Count > 0 ? "?" + string.Join("&", parts) : "";
        }

        // ── Mappery API → Model ───────────────────────────────────────────────

        private static Platnosc MapPlatnosc(ApiPlatnosc a) => new()
        {
            Id              = a.Id?.ToString() ?? "",
            Tytul           = a.Title ?? a.Description ?? "",
            Typ             = ParseTypPlatnosci(a.Type),
            Klient          = a.Client ?? a.ClientName ?? "",
            KontoNazwa      = a.AccountName ?? "",
            NumerRezerwacji = a.ReservationNo ?? a.ReservationNumber ?? "—",
            Data            = a.Date ?? a.CreatedAt ?? DateTime.Now,
            Kwota           = a.Amount ?? a.Value ?? 0m,
            Przychod        = a.Income ?? (a.Direction?.ToLower() == "in")
        };

        private static Dokument MapDokument(ApiDokument a) => new()
        {
            Id              = a.Id?.ToString() ?? "",
            NumerRezerwacji = a.ReservationNo ?? a.ReservationNumber ?? "",
            Numer           = a.Number ?? a.DocumentNumber ?? "",
            Typ             = ParseTypDokumentu(a.Type),
            Klient          = a.Client ?? a.ClientName ?? "",
            KwotaBrutto     = a.GrossAmount ?? a.Amount ?? 0m,
            KwotaWplaty     = a.PaidAmount ?? 0m,
            DataWystawienia = a.IssueDate ?? a.CreatedAt ?? DateTime.Now,
            DataSprzedazy   = a.SaleDate,
            DataPlatnosci   = a.PaymentDate
        };

        private static KontoFinansowe MapKonto(ApiKonto a) => new()
        {
            Id             = a.Id?.ToString() ?? "",
            Nazwa          = a.Name ?? "",
            Typ            = ParseTypKonta(a.TypeInt),
            OrganizationId = a.OrganizationId ?? 0,
            UserId         = a.UserId ?? 0,
            DateModified   = a.DateModified ?? "",
            Aktywne        = true
        };

        private static TypPlatnosci ParseTypPlatnosci(string? t) => t?.ToLower() switch
        {
            "przelew" or "transfer" or "bank_transfer" => TypPlatnosci.Przelew,
            "gotowka" or "cash"                        => TypPlatnosci.Gotowka,
            "karta"   or "card"                        => TypPlatnosci.Karta,
            "online"                                   => TypPlatnosci.Online,
            "zwrot"   or "refund" or "return"          => TypPlatnosci.Zwrot,
            _                                          => TypPlatnosci.Przelew
        };

        private static TypDokumentu ParseTypDokumentu(string? t) => t?.ToLower() switch
        {
            "faktura" or "invoice" or "vat" => TypDokumentu.Faktura,
            "paragon" or "receipt"          => TypDokumentu.Paragon,
            "korekta" or "correction"       => TypDokumentu.Korekta,
            "nota"    or "note"             => TypDokumentu.Nota,
            _                               => TypDokumentu.Faktura
        };

        private static TypKonta ParseTypKonta(int? t) => t switch
        {
            0 => TypKonta.Gotowkowe,
            1 => TypKonta.Bankowe,
            2 => TypKonta.Karta,
            _ => TypKonta.Inne
        };
    }
}
