using System.Text.Json.Serialization;

namespace yBook.Models
{
    public class Reservation
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("date_modified")]
        public string DateModified { get; set; }

        [JsonPropertyName("status_id")]
        public int StatusId { get; set; }

        [JsonPropertyName("client_id")]
        public int ClientId { get; set; }

        [JsonPropertyName("document_id")]
        public int DocumentId { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        [JsonPropertyName("is_removed")]
        public int IsRemoved { get; set; }

        [JsonPropertyName("pre_payment")]
        public string PrePayment { get; set; }

        [JsonPropertyName("pre_payment_date")]
        public string PrePaymentDate { get; set; }

        [JsonPropertyName("discount_amount")]
        public string DiscountAmount { get; set; }

        [JsonPropertyName("discount_type")]
        public int DiscountType { get; set; }

        [JsonPropertyName("discount_type_amount")]
        public string DiscountTypeAmount { get; set; }

        [JsonPropertyName("date_created")]
        public string DateCreated { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("is_checked_in")]
        public int IsCheckedIn { get; set; }

        [JsonPropertyName("checked_in_date")]
        public string CheckedInDate { get; set; }

        [JsonPropertyName("is_paid")]
        public int IsPaid { get; set; }

        [JsonPropertyName("is_invoice")]
        public int IsInvoice { get; set; }

        [JsonPropertyName("info_for_client")]
        public string InfoForClient { get; set; }

        [JsonPropertyName("created_by")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("online_need_invoice")]
        public int OnlineNeedInvoice { get; set; }

        [JsonPropertyName("total_amount")]
        public string TotalAmount { get; set; }

        [JsonPropertyName("no_reminder")]
        public int NoReminder { get; set; }

        [JsonPropertyName("no_survey")]
        public int NoSurvey { get; set; }

        [JsonPropertyName("survey_completed")]
        public int SurveyCompleted { get; set; }

        [JsonPropertyName("was_checked_in")]
        public int WasCheckedIn { get; set; }
    }

    public class ReservationResponse
    {
        [JsonPropertyName("items")]
        public Reservation[] Items { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
