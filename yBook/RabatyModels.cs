namespace yBook.Models
{
    public class Rabat
    {
        public string Nazwa { get; set; }
        public string Kod { get; set; }
        public string Opis { get; set; }
        public double Procent { get; set; }
        public DateTime DataWaznosci { get; set; }
        public bool CzyOnline { get; set; }

        public string ProcentStr => $"{Procent}%";
        public string DataStr => DataWaznosci.ToString("dd.MM.yyyy");
    }
}