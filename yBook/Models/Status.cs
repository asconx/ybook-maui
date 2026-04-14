namespace yBook.Models;

public class Status
{
    public int Id { get; set; }
    public string Nazwa { get; set; } = string.Empty;
    public string Kolor { get; set; } = "#3498db";
    public string Opis { get; set; } = string.Empty;
    public List<string> Powiadomienia { get; set; } = new();

    public string PowiadomieniaText => Powiadomienia.Count > 0
        ? string.Join(", ", Powiadomienia)
        : "Brak powiadomień";
}