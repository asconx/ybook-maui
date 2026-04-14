using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace yBook.Models
{
    public class FinancialListResponse
    {
        [JsonPropertyName("items")]
        public List<FinancialItemDto> Items { get; set; } = new();

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class FinancialItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("amount_gross")]
        public string? AmountGross { get; set; }

        [JsonPropertyName("amount_net")]
        public string? AmountNet { get; set; }

        [JsonPropertyName("amount_vat")]
        public string? AmountVat { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("date_modified")]
        public string? DateModified { get; set; }

        [JsonPropertyName("document")]
        public DocumentDto? Document { get; set; }
    }

    public class DocumentDto
    {
        [JsonPropertyName("document_number")]
        public string? DocumentNumber { get; set; }

        [JsonPropertyName("issue_date")]
        public string? IssueDate { get; set; }

        [JsonPropertyName("client_name")]
        public string? ClientName { get; set; }
    }

    public class FinancialCreateDto
    {
        [JsonPropertyName("service_id")]
        public int ServiceId { get; set; }

        [JsonPropertyName("amount_gross")]
        public decimal AmountGross { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("issue_date")]
        public string? IssueDate { get; set; }

        [JsonPropertyName("document_number")]
        public string? DocumentNumber { get; set; }
    }
}
