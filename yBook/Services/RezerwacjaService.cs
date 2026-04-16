using yBook.Models;

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
                    Id = "0520",
                    Imie = "Jan",
                    Nazwisko = "Kowalski",
                    TypPokoju = "Pokój Dwuosobowy typu Deluxe 8",
                    DataPrzyjazdu = DateTime.Today,
                    DataWyjazdu = DateTime.Today.AddDays(4),
                    LiczbaGosci = 2,
                    Status = StatusRezerwacji.Potwierdzona,
                    Email = "jan.kowalski@example.com",
                    Telefon = "500-500-500",
                    Uwagi = "Proszê nie przeszkadzaæ rano",
                    Rozliczenie = "7 dni przed przyjazdem"
                },
                new RezerwacjaOnline
                {
                    Id = "0572",
                    Imie = "Anna",
                    Nazwisko = "Nowak",
                    TypPokoju = "Pokój Dwuosobowy typu Deluxe 6",
                    DataPrzyjazdu = DateTime.Today.AddDays(1),
                    DataWyjazdu = DateTime.Today.AddDays(11),
                    LiczbaGosci = 2,
                    Status = StatusRezerwacji.Potwierdzona,
                    Email = "anna.nowak@example.com",
                    Telefon = "501-501-501",
                    Uwagi = "Wczesny check-in",
                    Rozliczenie = "Przy przyjezdzie"
                }
            };
        }

        public async Task<List<RezerwacjaOnline>> GetRezerwacjeZameldowaneAsync()
        {
            try
            {
                var rezerwacje = await _apiClient.GetAsync<List<RezerwacjaOnline>>("/activeReservation");
                return rezerwacje
                    .Where(r => r.DataPrzyjazdu <= DateTime.Today && r.DataWyjazdu > DateTime.Today)
                    .ToList();
            }
            catch
            {
                return _localCache
                    .Where(r => r.DataPrzyjazdu <= DateTime.Today && r.DataWyjazdu > DateTime.Today)
                    .ToList();
            }
        }

        public async Task<List<RezerwacjaOnline>> GetRezerwacjeNiezameldowaneAsync()
        {
            try
            {
                var rezerwacje = await _apiClient.GetAsync<List<RezerwacjaOnline>>("/activeReservation");
                return rezerwacje
                    .Where(r => r.DataPrzyjazdu > DateTime.Today)
                    .ToList();
            }
            catch
            {
                return _localCache
                    .Where(r => r.DataPrzyjazdu > DateTime.Today)
                    .ToList();
            }
        }

        public async Task<List<RezerwacjaOnline>> GetAllRezerwacjeAsync()
        {
            try
            {
                return await _apiClient.GetAsync<List<RezerwacjaOnline>>("/activeReservation");
            }
            catch
            {
                return _localCache.ToList();
            }
        }

        public async Task<RezerwacjaOnline> GetRezerwacjaByIdAsync(string id)
        {
            try
            {
                var rezerwacje = await _apiClient.GetAsync<List<RezerwacjaOnline>>("/activeReservation");
                return rezerwacje.FirstOrDefault(r => r.Id == id);
            }
            catch
            {
                return _localCache.FirstOrDefault(r => r.Id == id);
            }
        }

        public async Task<bool> UpdateRezerwacjaAsync(RezerwacjaOnline rezerwacja)
        {
            try
            {
                await _apiClient.PutAsync<RezerwacjaOnline>($"/reservation/{rezerwacja.Id}", rezerwacja);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}