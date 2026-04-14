namespace yBook.Models
{
    public class Rabat
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public string Kod { get; set; }
        public string Opis { get; set; }
        public int Procent { get; set; }
        public bool CzyOnline { get; set; }
        public DateTime DataWaznosci { get; set; }

        public string ProcentStr => $"{Procent}%";
        public string DataStr => DataWaznosci == DateTime.MinValue ? "Brak daty" : DataWaznosci.ToString("dd.MM.yyyy");
    }
}