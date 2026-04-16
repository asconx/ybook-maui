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
        private const string API_URL = "/activeReservation";

        public RezerwacjaService(IAuthService authService = null)
        {
            _apiClient = new ApiClient(authService);
        }

        private List<RezerwacjaOnline> MapApiResponseToRezerwacje(JsonElement response)
        {
            var rezerwacje = new List<RezerwacjaOnline>();
            System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] MapApiResponseToRezerwacje called");
            System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] Response type: {response.ValueKind}");

            try
            {
                if (response.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    var count = items.GetArrayLength();
                    System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] Found 'items' array with {count} elements");

                    foreach (var item in items.EnumerateArray())
                    {
                        var rez = MapSingleItem(item);
                        if (rez != null)
                        {
                            rezerwacje.Add(rez);
                            System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] Mapped reservation: {rez.Id} - {rez.Imie} {rez.Nazwisko}");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] Total mapped: {rezerwacje.Count}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] 'items' property not found or not array. Response keys: {string.Join(", ", response.EnumerateObject().Select(p => p.Name))}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] ERROR in MapApiResponseToRezerwacje: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] Stack: {ex.StackTrace}");
            }

            return rezerwacje;
        }

        private RezerwacjaOnline MapSingleItem(JsonElement item)
        {
            try
            {
                // Nowa struktura API - properties są bezpośrednio w item
                var dateFrom = item.GetProperty("date_from").GetString();
                var dateTo = item.GetProperty("date_to").GetString();
                var booked_by_external_name = item.GetProperty("booked_by_external_name").GetString() ?? "";

                // Optionally get nested objects if they exist
                var room = item.TryGetProperty("room", out var roomElement) ? roomElement : default;
                var client = item.TryGetProperty("client", out var clientElement) ? clientElement : default;
                var reservation = item.TryGetProperty("reservation", out var reservationElement) ? reservationElement : default;

                var nameParts = booked_by_external_name.Split(' ');

                var rezerwacja = new RezerwacjaOnline
                {
                    Id = item.GetProperty("id").GetInt32().ToString(),
                    Imie = nameParts.Length > 0 ? nameParts[0] : "",
                    Nazwisko = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                    TypPokoju = room.ValueKind != JsonValueKind.Undefined && room.TryGetProperty("name", out var roomName) 
                        ? roomName.GetString() ?? "" 
                        : "",
                    DataPrzyjazdu = DateTime.Parse(dateFrom ?? DateTime.Today.ToString()),
                    DataWyjazdu = DateTime.Parse(dateTo ?? DateTime.Today.ToString()),
                    Telefon = client.ValueKind != JsonValueKind.Undefined && client.TryGetProperty("phone", out var phone)
                        ? phone.GetString() ?? ""
                        : "",
                    Email = client.ValueKind != JsonValueKind.Undefined && client.TryGetProperty("email", out var email)
                        ? email.GetString() ?? ""
                        : "",
                    Status = StatusRezerwacji.Potwierdzona,
                    Uwagi = reservation.ValueKind != JsonValueKind.Undefined && reservation.TryGetProperty("notes", out var notes)
                        ? notes.GetString() ?? ""
                        : ""
                };

                System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] Successfully mapped item {rezerwacja.Id}");
                return rezerwacja;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] ERROR in MapSingleItem: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[RezerwacjaService] Item properties: {string.Join(", ", item.EnumerateObject().Select(p => p.Name))}");
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
