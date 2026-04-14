using System.Linq;

namespace yBook.Models
{
    public class User
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Role { get; set; } = "";
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateModified { get; set; }
        public DateTime LastLogin { get; set; }

        // Notification settings - parsed from comma-separated string
        public bool NowaPlatnosc { get; set; }
        public bool WyslijPowiadomienieKlient { get; set; }
        public bool AnulowanieRezerwacji { get; set; }
        public bool NowaRezerwacjaOnline { get; set; }
        public bool SynchronizacjaRezerwacji { get; set; }
        public bool UtworzenieNowejRezerwacji { get; set; }

        // Parse notification settings from API string format
        public void ParseNotificationSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings)) return;

            var settingsArray = settings.Split(',').Select(s => s.Trim()).ToArray();

            NowaPlatnosc = settingsArray.Contains("new_online_booking");
            WyslijPowiadomienieKlient = settingsArray.Contains("notification_client");
            AnulowanieRezerwacji = settingsArray.Contains("cancel_reservation");
            NowaRezerwacjaOnline = settingsArray.Contains("new_online_booking");
            SynchronizacjaRezerwacji = settingsArray.Contains("sync_reservation");
            UtworzenieNowejRezerwacji = settingsArray.Contains("new_reservation");
        }

        // Convert notification settings to API format
        public string GetNotificationSettingsString()
        {
            var items = new List<string>();
            if (UtworzenieNowejRezerwacji) items.Add("new_reservation");
            if (SynchronizacjaRezerwacji) items.Add("sync_reservation");
            if (NowaRezerwacjaOnline) items.Add("new_online_booking");
            if (AnulowanieRezerwacji) items.Add("cancel_reservation");
            if (WyslijPowiadomienieKlient) items.Add("notification_client");
            if (NowaPlatnosc) items.Add("new_online_booking");

            return string.Join(",", items);
        }

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