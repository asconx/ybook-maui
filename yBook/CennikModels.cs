namespace yBook.Models
{
    // ─── Model cennika ────────────────────────────────────────────────────────

    public record CennikItem
    {
        public string       Id        { get; init; } = "";
        public string       Nazwa     { get; init; } = "";
        public decimal      Cena      { get; init; }
        public int          Priorytet { get; init; } = 1;
        public DateTime     Od        { get; init; }
        public DateTime     Do        { get; init; }
        public List<string> Pokoje    { get; init; } = new();

        // ── Właściwości widoku ────────────────────────────────────────────────

        public string CenaStr
            => $"{Cena:N2}";

        public string ZakresStr
            => $"{Od:yyyy-MM-dd} – {Do:yyyy-MM-dd}";

        public string PierwszyPokoj
            => Pokoje.Count > 0 ? Pokoje[0] : "—";

        public bool MaWiecejPokoi
            => Pokoje.Count > 1;

        public string DodatkowePokjeStr
            => Pokoje.Count > 1 ? $"+{Pokoje.Count - 1}" : "";
    }

    // ─── Mock factory ─────────────────────────────────────────────────────────

    public static class MockCennik
    {
        public static List<CennikItem> Cenniki() => new()
        {
            new() { Id = "1",  Nazwa = "Długi weekend sierpniowy", Cena = 420.00m, Priorytet = 1, Od = new(2023,8,11), Do = new(2023,8,16), Pokoje = new() { "Mały pokój 1" } },
            new() { Id = "2",  Nazwa = "Długi weekend sierpniowy", Cena = 520.00m, Priorytet = 1, Od = new(2023,8,11), Do = new(2023,8,16), Pokoje = new() { "Pokój dwuosobowy typu Standard 2" } },
            new() { Id = "3",  Nazwa = "Długi weekend sierpniowy", Cena = 920.00m, Priorytet = 1, Od = new(2023,8,11), Do = new(2023,8,16), Pokoje = new() { "Pokój czterosobowy typu Classic 3" } },
            new() { Id = "4",  Nazwa = "Długi weekend sierpniowy", Cena = 520.00m, Priorytet = 1, Od = new(2023,8,11), Do = new(2023,8,16), Pokoje = new() { "Pokój Dwuosobowy typu Economy 4" } },
            new() { Id = "5",  Nazwa = "Długi weekend sierpniowy", Cena = 920.00m, Priorytet = 1, Od = new(2023,8,11), Do = new(2023,8,16), Pokoje = new() { "Pokój czterosobowy typu Comfort 5" } },
            new() { Id = "6",  Nazwa = "Długi weekend sierpniowy", Cena = 600.00m, Priorytet = 1, Od = new(2023,8,11), Do = new(2023,8,16), Pokoje = new() { "Pokój Dwuosobowy typu Deluxe 6" } },
            new() { Id = "7",  Nazwa = "Długi weekend sierpniowy", Cena = 520.00m, Priorytet = 1, Od = new(2023,8,11), Do = new(2023,8,16), Pokoje = new() { "Pokój Dwuosobowy typu Deluxe 7" } },
            new() { Id = "8",  Nazwa = "Długi weekend sierpniowy", Cena = 287.00m, Priorytet = 1, Od = new(2023,8,11), Do = new(2023,8,16), Pokoje = new() { "Pokój Dwuosobowy typu Deluxe 8" } },
            new() { Id = "9",  Nazwa = "Emilia Krzynówek",         Cena = 340.00m, Priorytet = 1, Od = new(2023,7,19), Do = new(2023,7,22), Pokoje = new() { "Mały pokój 1", "Pokój dwuosobowy typu Standard 2", "Pokój czterosobowy typu Classic 3", "Pokój Dwuosobowy typu Economy 4" } },
            new() { Id = "10", Nazwa = "wrzesień",                 Cena = 246.00m, Priorytet = 1, Od = new(2023,9,1),  Do = new(2023,10,31), Pokoje = new() { "Mały pokój 1" } },
            new() { Id = "11", Nazwa = "wrzesień",                 Cena = 279.00m, Priorytet = 1, Od = new(2023,9,1),  Do = new(2023,10,31), Pokoje = new() { "Pokój dwuosobowy typu Standard 2" } },
            new() { Id = "12", Nazwa = "wrzesień",                 Cena = 370.00m, Priorytet = 1, Od = new(2023,9,1),  Do = new(2023,10,31), Pokoje = new() { "Pokój czterosobowy typu Classic 3" } },
        };
    }
}
