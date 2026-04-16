namespace yBook.Models;

public class Status
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Nazwa { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("color")]
    public string Kolor { get; set; } = "#3498db";

    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string Opis { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("organization_id")]
    public int? OrganizationId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("notification_id")]
    public int? NotificationId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("date_modified")]
    public string? DateModified { get; set; }

    // Legacy/UX field - not bound to API directly
    public List<string> Powiadomienia { get; set; } = new();

    public string PowiadomieniaText => Powiadomienia.Count > 0
        ? string.Join(", ", Powiadomienia)
        : "Brak powiadomień";
}