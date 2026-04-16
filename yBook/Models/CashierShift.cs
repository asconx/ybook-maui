using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace yBook.Models
{
    public class CashierShift
    {
        [JsonPropertyName("cashier_id")]
        public int CashierId { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDateRaw { get; set; } = "";

        [JsonPropertyName("finish_date")]
        public string FinishDateRaw { get; set; } = "";

        [JsonPropertyName("balance_cash")]
        public string BalanceCashRaw { get; set; } = "0.00";

        [JsonPropertyName("balance_bank")]
        public string BalanceBankRaw { get; set; } = "0.00";

        [JsonPropertyName("token")]
        public string Token { get; set; } = "";

        // --- pomocnicze, nie serializowane ---
        [JsonIgnore]
        public DateTime? StartDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(StartDateRaw)) return null;
                if (DateTime.TryParse(StartDateRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
                if (DateTime.TryParseExact(StartDateRaw, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
                return null;
            }
        }

        [JsonIgnore]
        public DateTime? FinishDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FinishDateRaw)) return null;
                if (DateTime.TryParse(FinishDateRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
                if (DateTime.TryParseExact(FinishDateRaw, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
                return null;
            }
        }

        [JsonIgnore]
        public string StartDateStr => StartDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "—";

        [JsonIgnore]
        public string FinishDateStr => FinishDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "—";

        [JsonIgnore]
        public decimal BalanceCash => decimal.TryParse(BalanceCashRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var c) ? c : 0m;

        [JsonIgnore]
        public decimal BalanceBank => decimal.TryParse(BalanceBankRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var b) ? b : 0m;

        [JsonIgnore]
        public string BalanceCashStr => BalanceCash.ToString("N2", CultureInfo.InvariantCulture);

        [JsonIgnore]
        public string BalanceBankStr => BalanceBank.ToString("N2", CultureInfo.InvariantCulture);
    }
}