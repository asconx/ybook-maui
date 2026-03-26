namespace yBook.Models
{
    public class PrzyjazdWyjazd
    {
        public string Pokoj { get; set; }

        public DateTime Data { get; set; }

        public bool PrzyjazdMozliwy { get; set; } = true;
        public bool WyjazdMozliwy { get; set; } = true;

        public string DataStr => Data.ToString("dd.MM.yyyy");

        public string Status =>
            $"{(PrzyjazdMozliwy ? "✔ Przyjazd" : "✖ Przyjazd")} | " +
            $"{(WyjazdMozliwy ? "✔ Wyjazd" : "✖ Wyjazd")}";
    }
}