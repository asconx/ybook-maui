using System.Linq;

namespace yBook.Models
{
    public class User
    {
        // Dodane Id
        public int Id { get; set; }

        public string Name { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Uprawnienia (przyk³ad, mo¿esz rozszerzyæ)
        public bool NowaPlatnosc { get; set; }
        public bool WyslijPowiadomienieKlient { get; set; }
        public bool AnulowanieRezerwacji { get; set; }
        public bool NowaRezerwacjaOnline { get; set; }
        public bool SynchronizacjaRezerwacji { get; set; }
        public bool UtworzenieNowejRezerwacji { get; set; }

        // Zwraca wybrane uprawnienia jako tekst rozdzielony przecinkami
        public string Permissions
        {
            get
            {
                var items = new[]
                {
                    NowaPlatnosc ? "Nowa p³atnoœæ online" : null,
                    WyslijPowiadomienieKlient ? "Powiadomienie dla klienta" : null,
                    AnulowanieRezerwacji ? "Anulowanie rezerwacji" : null,
                    NowaRezerwacjaOnline ? "Nowa rezerwacja online" : null,
                    SynchronizacjaRezerwacji ? "Synchronizacja rezerwacji" : null,
                    UtworzenieNowejRezerwacji ? "Utworzenie nowej rezerwacji" : null
                }.Where(s => !string.IsNullOrEmpty(s));

                return string.Join(", ", items);
            }
        }

        public override string ToString() => $"{Name} ({Role})";
    }
}