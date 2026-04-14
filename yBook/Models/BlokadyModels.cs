namespace yBook.Models
{
    public class Blokada
    {
        public string Nazwa { get; set; }
        public string Notatka { get; set; }

        public DateTime DataOd { get; set; }
        public DateTime DataDo { get; set; }

        public bool DlaWszystkich { get; set; }

        public List<string> Pokoje { get; set; } = new();

        public string DataZakres => $"{DataOd:dd.MM} - {DataDo:dd.MM}";
        public string PokojeStr => DlaWszystkich ? "Wszystkie" : string.Join(", ", Pokoje);
    }
}