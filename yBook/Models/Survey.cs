using System.Text.Json.Serialization;

namespace yBook.Models;

public class Survey
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("organization_id")]
    public int OrganizationId { get; set; }

    [JsonPropertyName("date_modified")]
    public string DateModified { get; set; } = string.Empty;

    [JsonPropertyName("notification_id")]
    public int NotificationId { get; set; }

    [JsonPropertyName("aspect_1")]
    public string Aspect1 { get; set; } = string.Empty;

    [JsonPropertyName("aspect_2")]
    public string Aspect2 { get; set; } = string.Empty;

    [JsonPropertyName("aspect_3")]
    public string Aspect3 { get; set; } = string.Empty;

    [JsonPropertyName("aspect_4")]
    public string Aspect4 { get; set; } = string.Empty;

    [JsonPropertyName("aspect_5")]
    public string Aspect5 { get; set; } = string.Empty;

    [JsonPropertyName("aspect_6")]
    public string Aspect6 { get; set; } = string.Empty;

    [JsonPropertyName("aspect_7")]
    public string Aspect7 { get; set; } = string.Empty;

    [JsonPropertyName("question_1")]
    public string Question1 { get; set; } = string.Empty;

    [JsonPropertyName("question_2")]
    public string Question2 { get; set; } = string.Empty;

    [JsonPropertyName("question_3")]
    public string Question3 { get; set; } = string.Empty;
}
