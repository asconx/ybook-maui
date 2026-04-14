namespace yBook.Models
{
    public class KalendarzPokoj
    {
        public int    Id      { get; set; }
        public string Nazwa   { get; set; } = string.Empty;
        public List<KalendarzRezerwacja> Rezerwacje { get; set; } = [];
    }

    public class KalendarzRezerwacja
    {
        public int      Id        { get; set; }
        public DateTime DataOd    { get; set; }
        public DateTime DataDo    { get; set; }
        public string   Opis      { get; set; } = string.Empty;
        public string   Kolor     { get; set; } = "#F5A623";
        public string   TextKolor { get; set; } = "#1A1A1A";

        // Wyświetlane w popupie
        public string NumerRezerwacji => $"Rezerwacja #{Id}";
        public string OkresStr        => $"{DataOd:yyyy-MM-dd} – {DataDo:yyyy-MM-dd}";
    }
}
