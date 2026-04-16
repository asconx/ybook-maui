using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using yBook.Models;
using yBook.Services;

namespace yBook.ViewModels;

public class StatusyViewModel : INotifyPropertyChanged
{
    // ── ObservableCollection ──────────────────────────────────────────────

    public ObservableCollection<Status> Statusy { get; } = new();

    public int LiczbaStatusow => Statusy.Count;

    // ── Pola formularza ───────────────────────────────────────────────────

    private string _nowaNazwa = string.Empty;
    public string NowaNazwa
    {
        get => _nowaNazwa;
        set { _nowaNazwa = value; Notify(); }
    }

    private string _nowyKolor = "#3498db";
    public string NowyKolor
    {
        get => _nowyKolor;
        set { _nowyKolor = value; Notify(); }
    }

    private string _nowyOpis = string.Empty;
    public string NowyOpis
    {
        get => _nowyOpis;
        set { _nowyOpis = value; Notify(); }
    }

    // ── Stan formularza ───────────────────────────────────────────────────

    private bool _formularzWidoczny;
    public bool FormularzWidoczny
    {
        get => _formularzWidoczny;
        set { _formularzWidoczny = value; Notify(); Notify(nameof(PrzyciskDodajWidoczny)); }
    }

    public bool PrzyciskDodajWidoczny => !FormularzWidoczny;

    private bool _czyEdycja;
    public string TytulFormularza => _czyEdycja ? "Edytuj status" : "Nowy status";

    // ── Komendy ───────────────────────────────────────────────────────────

    public ICommand PokazFormularzCommand { get; }
    public ICommand ZapiszCommand { get; }
    public ICommand AnulujCommand { get; }
    public ICommand EdytujCommand { get; }
    public ICommand UsunCommand { get; }

    // ── Prywatne ──────────────────────────────────────────────────────────

    private Status? _edytowany;
    private int _nextId = 1;
    private static readonly HttpClient _httpClient = new HttpClient();
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; Notify(); }
    }

    // ── Konstruktor ───────────────────────────────────────────────────────

    public StatusyViewModel()
    {
        PokazFormularzCommand = new Command(PokazFormularz);
        ZapiszCommand = new Command(Zapisz);
        AnulujCommand = new Command(Anuluj);
        EdytujCommand = new Command<Status>(Edytuj);
        UsunCommand = new Command<Status>(Usun);

        // Odśwież LiczbaStatusow przy każdej zmianie kolekcji
        Statusy.CollectionChanged += (_, _) => Notify(nameof(LiczbaStatusow));

        // Dane przykładowe
        Statusy.Add(new Status { Id = _nextId++, Nazwa = "Aktywny", Kolor = "#2ecc71", Opis = "Widoczny dla klientów" });
        Statusy.Add(new Status { Id = _nextId++, Nazwa = "Nieaktywny", Kolor = "#e74c3c", Opis = "Ukryty" });
    }

    // Asynchroniczne pobranie statusów z API (bez dodatkowych requestów ani GET po ID)
    public async Task LoadStatusesAsync()
    {
        if (IsBusy) return; // guard against reentrancy / double calls

        IsBusy = true;
        try
        {
            var url = "https://api.ybook.pl/entity/status"; // exact endpoint
            System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] GET {url}");

            // Try to read saved token via AuthService (uses SecureStorage internally)
            string? token = null;
            try
            {
                var auth = IPlatformApplication.Current?.Services.GetService<yBook.Services.IAuthService>();
                if (auth is not null)
                    token = await auth.GetTokenAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] Failed to get token from DI: {ex.Message}");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] Sending request to: {request.RequestUri}");

            var response = await _httpClient.SendAsync(request);
            System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] Response status: {response.StatusCode}");

            var responseContent = string.Empty;
            try
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] Failed to read response content: {ex}");
            }

            System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] Response body (truncated): {responseContent.Substring(0, Math.Min(1000, responseContent.Length))}");

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] Error status: {(int)response.StatusCode} {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] Error body: {responseContent}");
                return;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            List<ApiStatusDto> items = new List<ApiStatusDto>();

            try
            {
                var wrapper = JsonSerializer.Deserialize<ApiResponseDto>(responseContent, options);
                items = wrapper?.Items ?? new List<ApiStatusDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] JSON parsing error (expecting wrapper with 'items'): {ex}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] Items count from API: {items.Count}");
            if (items.Count > 0)
            {
                var first = items[0];
                System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] First item - id: {first.Id}, name: {first.Name}");
            }

            // Replace collection contents (avoid UI double requests)
            Statusy.Clear();
            foreach (var it in items)
            {
                var s = new Status
                {
                    Id = it.Id,
                    Nazwa = it.Name ?? string.Empty,
                    Kolor = string.IsNullOrEmpty(it.Color) ? "#3498db" : it.Color,
                    Opis = it.Description ?? string.Empty,
                    OrganizationId = it.OrganizationId,
                    NotificationId = it.NotificationId,
                    DateModified = it.DateModified
                };

                Statusy.Add(s);
            }

            _nextId = Statusy.Any() ? Statusy.Max(s => s.Id) + 1 : 1;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StatusyViewModel] LoadStatusesAsync error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Logika ────────────────────────────────────────────────────────────

    private void PokazFormularz()
    {
        WyczyscFormularz();
        _czyEdycja = false;
        Notify(nameof(TytulFormularza));
        FormularzWidoczny = true;
    }

    private void Zapisz()
    {
        if (string.IsNullOrWhiteSpace(NowaNazwa)) return;

        if (_czyEdycja && _edytowany is not null)
        {
            // Podmień obiekt w kolekcji → CollectionView się odświeży
            int idx = Statusy.IndexOf(_edytowany);
            if (idx >= 0)
                Statusy[idx] = new Status
                {
                    Id = _edytowany.Id,
                    Nazwa = NowaNazwa,
                    Kolor = NowyKolor,
                    Opis = NowyOpis,
                    Powiadomienia = _edytowany.Powiadomienia
                };
        }
        else
        {
            Statusy.Add(new Status
            {
                Id = _nextId++,
                Nazwa = NowaNazwa,
                Kolor = NowyKolor,
                Opis = NowyOpis
            });
        }

        FormularzWidoczny = false;
        WyczyscFormularz();
    }

    private void Anuluj()
    {
        FormularzWidoczny = false;
        WyczyscFormularz();
    }

    private void Edytuj(Status? s)
    {
        if (s is null) return;

        _edytowany = s;
        NowaNazwa = s.Nazwa;
        NowyKolor = s.Kolor;
        NowyOpis = s.Opis;
        _czyEdycja = true;
        Notify(nameof(TytulFormularza));
        FormularzWidoczny = true;
    }

    private void Usun(Status? s)
    {
        if (s is not null) Statusy.Remove(s);
    }

    private void WyczyscFormularz()
    {
        NowaNazwa = string.Empty;
        NowyKolor = "#3498db";
        NowyOpis = string.Empty;
        _edytowany = null;
    }

    // DTOs for API mapping
    private class ApiResponseDto
    {
        [JsonPropertyName("items")]
        public List<ApiStatusDto>? Items { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    private class ApiStatusDto
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

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("notification_id")]
        public int? NotificationId { get; set; }
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}