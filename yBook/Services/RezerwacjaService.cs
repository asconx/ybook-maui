using yBook.Models;
using System.Text.Json;

namespace yBook.Services
{
    public interface IRezerwacjaService
    {
        Task<List<RezerwacjaOnline>> GetRezerwacjeZameldowaneAsync();
        Task<List<RezerwacjaOnline>> GetRezerwacjeNiezameldowaneAsync();
        Task<List<RezerwacjaOnline>> GetAllRezerwacjeAsync();
        Task<RezerwacjaOnline> GetRezerwacjaByIdAsync(string id);
        Task<bool> UpdateRezerwacjaAsync(RezerwacjaOnline rezerwacja);
    }

    public class RezerwacjaService : IRezerwacjaService
    {
        private readonly ApiClient _apiClient;
        private List<RezerwacjaOnline> _localCache = new();
        private const string API_URL = "https://api.ybook.pl/activeReservation";

        public RezerwacjaService()
        {
            _apiClient = new ApiClient();
            InitializeLocalData();
        }

        private void InitializeLocalData()
        {
            _localCache = new List<RezerwacjaOnline>
            {
                new RezerwacjaOnline
                {
                    Id = "6488",
                    Imie = "booking2",
                    Nazwisko = "CLOSED - Not available",
                    TypPokoju = "Pokój Dwuosobowy typu Deluxe 8",
                    DataPrzyjazdu = new DateTime(2026, 4, 16),
                    DataWyjazdu = new DateTime(2026, 4, 26),
                    LiczbaGosci = 0,
                    Status = StatusRezerwacji.Oczekujaca,
                    Email = "",
                    Telefon = "111111111",
                    Uwagi = "",
                    Rozliczenie = "7 dni przed przyjazdem"
                }
            };
        }

        private List<RezerwacjaOnline> MapApiResponseToRezerwacje(JsonElement response)
        {
            var rezerwacje = new List<RezerwacjaOnline>();

            try
            {
                if (response.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        var rez = MapSingleItem(item);
                        if (rez != null)
                            rezerwacje.Add(rez);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd mapowania API response: {ex.Message}");
            }

            return rezerwacje;
        }

        private RezerwacjaOnline MapSingleItem(JsonElement item)
        {
            try
            {
                var reservation = item.GetProperty("reservation");
                var room = item.GetProperty("room");
                var client = item.GetProperty("client");
                var dateFrom = item.GetProperty("date_from").GetString();
                var dateTo = item.GetProperty("date_to").GetString();

                var fullName = client.GetProperty("name").GetString() ?? "";
                var nameParts = fullName.Split(' ');

                var rezerwacja = new RezerwacjaOnline
                {
                    Id = reservation.GetProperty("id").GetString() ?? "",
                    Imie = nameParts.Length > 0 ? nameParts[0] : "",
                    Nazwisko = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                    TypPokoju = room.GetProperty("name").GetString() ?? "",
                    DataPrzyjazdu = DateTime.Parse(dateFrom ?? DateTime.Today.ToString()),
                    DataWyjazdu = DateTime.Parse(dateTo ?? DateTime.Today.ToString()),
                    Telefon = client.GetProperty("phone").GetString() ?? "",
                    Email = client.GetProperty("email").GetString() ?? "",
                    Status = StatusRezerwacji.Potwierdzona,
                    Uwagi = reservation.GetProperty("notes").GetString() ?? ""
                };

                return rezerwacja;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd mapowania elementu: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RezerwacjaOnline>> GetRezerwacjeZameldowaneAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<JsonElement>(API_URL);
                var rezerwacje = MapApiResponseToRezerwacje(response);
                
                var today = DateTime.Today;
                return rezerwacje
                    .Where(r => r.DataPrzyjazdu <= today && r.DataWyjazdu > today)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas pobierania rezerwacji zameldowanych: {ex.Message}");
                return _localCache
                    .Where(r => r.DataPrzyjazdu <= DateTime.Today && r.DataWyjazdu > DateTime.Today)
                    .ToList();
            }
        }

        public async Task<List<RezerwacjaOnline>> GetRezerwacjeNiezameldowaneAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<JsonElement>(API_URL);
                var rezerwacje = MapApiResponseToRezerwacje(response);
                
                return rezerwacje
                    .Where(r => r.DataPrzyjazdu > DateTime.Today)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas pobierania rezerwacji niezameldowanych: {ex.Message}");
                return _localCache
                    .Where(r => r.DataPrzyjazdu > DateTime.Today)
                    .ToList();
            }
        }

        public async Task<List<RezerwacjaOnline>> GetAllRezerwacjeAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<JsonElement>(API_URL);
                return MapApiResponseToRezerwacje(response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas pobierania wszystkich rezerwacji: {ex.Message}");
                return _localCache.ToList();
            }
        }

        public async Task<RezerwacjaOnline> GetRezerwacjaByIdAsync(string id)
        {
            try
            {
                var rezerwacje = await GetAllRezerwacjeAsync();
                return rezerwacje.FirstOrDefault(r => r.Id == id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas pobierania rezerwacji: {ex.Message}");
                return _localCache.FirstOrDefault(r => r.Id == id);
            }
        }

        public async Task<bool> UpdateRezerwacjaAsync(RezerwacjaOnline rezerwacja)
        {
            try
            {
                await _apiClient.PutAsync<RezerwacjaOnline>($"https://api.ybook.pl/reservation/{rezerwacja.Id}", rezerwacja);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas aktualizacji rezerwacji: {ex.Message}");
                return false;
            }
        }
    }
}
