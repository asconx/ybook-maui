namespace yBook.Models
{
    public enum TypAkcji
    {
        Logowanie,
        Wylogowanie,
        Dodanie,
        Edycja,
        Usuniecie,
        Eksport,
        Import,
        Inne
    }

    public class LogAkcji
    {
        public string Id { get; init; } = "";
        public DateTime Data { get; init; }
        public string Uzytkownik { get; init; } = "";
        public TypAkcji Typ { get; init; }
        public string Opis { get; init; } = "";
        public string AdresIp { get; init; } = "";

        public string DataStr => Data.ToString("dd.MM.yyyy HH:mm");
        public string TypLabel => Typ switch
        {
            TypAkcji.Logowanie => "Logowanie",
            TypAkcji.Wylogowanie => "Wylogowanie",
            TypAkcji.Dodanie => "Dodanie",
            TypAkcji.Edycja => "Edycja",
            TypAkcji.Usuniecie => "Usunięcie",
            TypAkcji.Eksport => "Eksport",
            TypAkcji.Import => "Import",
            _ => "Inne"
        };
        public string TypKolor => Typ switch
        {
            TypAkcji.Logowanie => "#4CAF50",
            TypAkcji.Wylogowanie => "#FF9800",
            TypAkcji.Dodanie => "#2196F3",
            TypAkcji.Edycja => "#9C27B0",
            TypAkcji.Usuniecie => "#F44336",
            TypAkcji.Eksport => "#00BCD4",
            TypAkcji.Import => "#607D8B",
            _ => "#9E9E9E"
        };
    }

    // ─── Dane testowe ──────────────────────────────────────────────────────────
    public static class MockLogi
    {
        public static List<LogAkcji> Logi() => new()
        {
            new() { Id="L001", Data=DateTime.Now.AddMinutes(-5),  Uzytkownik="admin",       Typ=TypAkcji.Logowanie,   Opis="Zalogowano do systemu",              AdresIp="192.168.1.10" },
            new() { Id="L002", Data=DateTime.Now.AddMinutes(-12), Uzytkownik="admin",       Typ=TypAkcji.Edycja,      Opis="Zmieniono dane rezerwacji #RZ-2024",  AdresIp="192.168.1.10" },
            new() { Id="L003", Data=DateTime.Now.AddMinutes(-30), Uzytkownik="recepcja1",   Typ=TypAkcji.Dodanie,     Opis="Dodano nowego klienta: Kowalski Jan", AdresIp="192.168.1.15" },
            new() { Id="L004", Data=DateTime.Now.AddHours(-1),    Uzytkownik="recepcja1",   Typ=TypAkcji.Logowanie,   Opis="Zalogowano do systemu",              AdresIp="192.168.1.15" },
            new() { Id="L005", Data=DateTime.Now.AddHours(-2),    Uzytkownik="manager",     Typ=TypAkcji.Eksport,     Opis="Eksport raportu płatności do PDF",   AdresIp="192.168.1.20" },
            new() { Id="L006", Data=DateTime.Now.AddHours(-3),    Uzytkownik="admin",       Typ=TypAkcji.Usuniecie,   Opis="Usunięto blokadę pokoju nr 5",       AdresIp="192.168.1.10" },
            new() { Id="L007", Data=DateTime.Now.AddHours(-4),    Uzytkownik="manager",     Typ=TypAkcji.Import,      Opis="Import danych MT940",                AdresIp="192.168.1.20" },
            new() { Id="L008", Data=DateTime.Now.AddHours(-5),    Uzytkownik="recepcja2",   Typ=TypAkcji.Logowanie,   Opis="Zalogowano do systemu",              AdresIp="192.168.1.30" },
            new() { Id="L009", Data=DateTime.Now.AddHours(-6),    Uzytkownik="recepcja2",   Typ=TypAkcji.Edycja,      Opis="Zmieniono cennik: Pokój Standard",   AdresIp="192.168.1.30" },
            new() { Id="L010", Data=DateTime.Now.AddHours(-8),    Uzytkownik="recepcja2",   Typ=TypAkcji.Wylogowanie, Opis="Wylogowano z systemu",               AdresIp="192.168.1.30" },
            new() { Id="L011", Data=DateTime.Now.AddDays(-1),     Uzytkownik="admin",       Typ=TypAkcji.Dodanie,     Opis="Dodano nową usługę: Parking",        AdresIp="192.168.1.10" },
            new() { Id="L012", Data=DateTime.Now.AddDays(-1),     Uzytkownik="manager",     Typ=TypAkcji.Edycja,      Opis="Zaktualizowano dane obiektu",        AdresIp="192.168.1.20" },
            new() { Id="L013", Data=DateTime.Now.AddDays(-2),     Uzytkownik="recepcja1",   Typ=TypAkcji.Usuniecie,   Opis="Usunięto rabat: Wczesniak",          AdresIp="192.168.1.15" },
            new() { Id="L014", Data=DateTime.Now.AddDays(-3),     Uzytkownik="admin",       Typ=TypAkcji.Eksport,     Opis="Eksport listy klientów",             AdresIp="192.168.1.10" },
            new() { Id="L015", Data=DateTime.Now.AddDays(-3),     Uzytkownik="manager",     Typ=TypAkcji.Logowanie,   Opis="Zalogowano do systemu",              AdresIp="192.168.1.20" },
        };
    }
}