using yBook.Models;

namespace yBook.Services
{
    public interface IRezerwacjaService
    {
        List<RezerwacjaOnline> GetRezerwacjeZameldowane();
        List<RezerwacjaOnline> GetRezerwacjeNiezameldowane();
        List<RezerwacjaOnline> GetAllRezerwacje();
        RezerwacjaOnline? GetRezerwacjaById(string id);
    }

    public class RezerwacjaService : IRezerwacjaService
    {
        private static List<RezerwacjaOnline> _rezerwacje = new();

        public RezerwacjaService()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            _rezerwacje = new List<RezerwacjaOnline>
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
                    OpcjaFaktury = false,
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
                    OpcjaFaktury = true,
                    Rozliczenie = "Przy przyjezdzie"
                },
                new RezerwacjaOnline
                {
                    Id = "0574",
                    Imie = "Piotr",
                    Nazwisko = "Lewandowski",
                    TypPokoju = "Pokój Dwuosobowy typu Deluxe 7",
                    DataPrzyjazdu = DateTime.Today,
                    DataWyjazdu = DateTime.Today.AddDays(4),
                    LiczbaGosci = 2,
                    Status = StatusRezerwacji.Potwierdzona,
                    Email = "piotr@example.com",
                    Telefon = "502-502-502",
                    Uwagi = "Dieta bezglutenowa",
                    OpcjaFaktury = false,
                    Rozliczenie = "7 dni przed przyjazdem"
                },
                new RezerwacjaOnline
                {
                    Id = "0587",
                    Imie = "Maria",
                    Nazwisko = "Wójcik",
                    TypPokoju = "Pokój Dwuosobowy typu Deluxe 9",
                    DataPrzyjazdu = DateTime.Today.AddDays(1),
                    DataWyjazdu = DateTime.Today.AddDays(6),
                    LiczbaGosci = 2,
                    Status = StatusRezerwacji.Oczekujaca,
                    Email = "maria.wojcik@example.com",
                    Telefon = "503-503-503",
                    Uwagi = "Wymaga parterowego pokoju",
                    OpcjaFaktury = true,
                    Rozliczenie = "Przedp³ata"
                },
                new RezerwacjaOnline
                {
                    Id = "0594",
                    Imie = "Tomasz",
                    Nazwisko = "Kamiñski",
                    TypPokoju = "Pokój czteroosobowy typu Classic 3",
                    DataPrzyjazdu = DateTime.Today,
                    DataWyjazdu = DateTime.Today.AddDays(2),
                    LiczbaGosci = 4,
                    Status = StatusRezerwacji.Potwierdzona,
                    Email = "tomasz@example.com",
                    Telefon = "504-504-504",
                    Uwagi = "",
                    OpcjaFaktury = false,
                    Rozliczenie = "7 dni przed przyjazdem"
                },
                new RezerwacjaOnline
                {
                    Id = "0600",
                    Imie = "Krzysztof",
                    Nazwisko = "Nowicki",
                    TypPokoju = "Pokój czteroosobowy typu 10",
                    DataPrzyjazdu = DateTime.Today.AddDays(1),
                    DataWyjazdu = DateTime.Today.AddDays(5),
                    LiczbaGosci = 3,
                    Status = StatusRezerwacji.Oczekujaca,
                    Email = "krzysztof@example.com",
                    Telefon = "505-505-505",
                    Uwagi = "Pobytu z prac¹ zdaln¹",
                    OpcjaFaktury = true,
                    Rozliczenie = "Przedp³ata"
                }
            };
        }

        public List<RezerwacjaOnline> GetRezerwacjeZameldowane()
        {
            // Zameldowani = ci co ju¿ przybyli (data przyjazdu <= dziœ) i jeszcze nie wyjechali
            return _rezerwacje
                .Where(r => r.DataPrzyjazdu <= DateTime.Today && r.DataWyjazdu > DateTime.Today)
                .ToList();
        }

        public List<RezerwacjaOnline> GetRezerwacjeNiezameldowane()
        {
            // Niezameldowani = ci co dopiero maj¹ przyjœæ
            return _rezerwacje
                .Where(r => r.DataPrzyjazdu > DateTime.Today)
                .ToList();
        }

        public List<RezerwacjaOnline> GetAllRezerwacje()
        {
            return _rezerwacje.ToList();
        }

        public RezerwacjaOnline? GetRezerwacjaById(string id)
        {
            return _rezerwacje.FirstOrDefault(r => r.Id == id);
        }
    }
}