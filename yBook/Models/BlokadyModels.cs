namespace yBook.Models
{
    public class Blokada
    {
        public int? Id { get; set; }  // API ID - null dla nowych blokad

        public string Nazwa { get; set; }
        public string Notatka { get; set; }

        public DateTime DataOd { get; set; }
        public DateTime DataDo { get; set; }

        public bool DlaWszystkich { get; set; }

        /// <summary>
        /// List of room names as strings for display
        /// </summary>
        public List<string> Pokoje { get; set; } = new();

        public string DataZakres => $"{DataOd:dd.MM} - {DataDo:dd.MM}";
        public string PokojeStr => DlaWszystkich ? "Wszystkie" : string.Join(", ", Pokoje);
    }
}