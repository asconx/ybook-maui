namespace yBook.Models;

public class RezerwacjaOnline
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    public DateTime DataZlozenia { get; set; } = DateTime.Now;

    // Dane gościa
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;

    // Pobyt
    public DateTime DataPrzyjazdu { get; set; } = DateTime.Today.AddDays(1);
    public DateTime DataWyjazdu { get; set; } = DateTime.Today.AddDays(2);
    public string TypPokoju { get; set; } = string.Empty;
    public int LiczbaGosci { get; set; } = 1;
    public string Uwagi { get; set; } = string.Empty;

    // Status
    public StatusRezerwacji Status { get; set; } = StatusRezerwacji.Oczekujaca;

    // Obliczane
    public int LiczbaNoci => Math.Max(0, (DataWyjazdu - DataPrzyjazdu).Days);
    public string PelneNazwisko => $"{Imie} {Nazwisko}".Trim();
    public string StatusLabel => Status switch
    {
        StatusRezerwacji.Oczekujaca  => "⏳ Oczekująca",
        StatusRezerwacji.Potwierdzona => "✅ Potwierdzona",
        StatusRezerwacji.Anulowana   => "❌ Anulowana",
        _                            => "–"
    };
    public Color StatusKolor => Status switch
    {
        StatusRezerwacji.Oczekujaca  => Color.FromArgb("#F57C00"),
        StatusRezerwacji.Potwierdzona => Color.FromArgb("#2E7D32"),
        StatusRezerwacji.Anulowana   => Color.FromArgb("#C62828"),
        _                            => Colors.Gray
    };
}

public enum StatusRezerwacji
{
    Oczekujaca,
    Potwierdzona,
    Anulowana
}
