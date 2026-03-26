namespace yBook.Models
{
    public class Pokoj
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int PropertyId { get; set; }
        public string DateModified { get; set; }
        public string Nazwa { get; set; }
        public int Type { get; set; }
        public bool CzyDostepny { get; set; }
        public int MaxOsobLiczbą { get; set; }
        public string Powierzchnia { get; set; }
        public string Opis { get; set; }
        public string ShortName { get; set; }
        public int DefaultPrice { get; set; }
        public string Kolor { get; set; }
        public string Standard { get; set; }
        public int MinOsobLiczbą { get; set; }
        public int LockId { get; set; }
        public int CalendarPosition { get; set; }

        public string StatusStr => CzyDostepny ? "✅ Dostępny" : "❌ Niedostępny";
        public string DetailStr => $"{Powierzchnia}m² • {MaxOsobLiczbą} osób";
    }
}
