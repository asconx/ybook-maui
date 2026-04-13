namespace yBook.Models
{
    public class PriceModifier
    {
        public int    Id              { get; set; }
        public int    OrganizationId  { get; set; }
        public string Name            { get; set; } = string.Empty;
        public string Description     { get; set; } = string.Empty;
        public string Amount          { get; set; } = "0.00";
        public string AmountAdult     { get; set; } = "0.00";
        public string AmountKid       { get; set; } = "0.00";
        public string AmountInfant    { get; set; } = "0.00";
        public string ApplyFromDate   { get; set; } = string.Empty;
        public string ApplyToDate     { get; set; } = string.Empty;
        public int    MinDays         { get; set; }
        public int    MaxDays         { get; set; }
        public int    Type            { get; set; }
        public int    Priority        { get; set; }

        // Flagi bool
        public bool IsMandatory     { get; set; }
        public bool IsService       { get; set; }
        public bool IsPercentAmount { get; set; }
        public bool IsPerPerson     { get; set; }
        public bool IsDaily         { get; set; }
        public bool IsForRoom       { get; set; }
        public bool IsForPeriod     { get; set; }
        public bool IsMonday        { get; set; }
        public bool IsTuesday       { get; set; }
        public bool IsWednesday     { get; set; }
        public bool IsThursday      { get; set; }
        public bool IsFriday        { get; set; }
        public bool IsSaturday      { get; set; }
        public bool IsSunday        { get; set; }
        public bool HideOnOnlineReservation { get; set; }
        public bool IncludeDiscount { get; set; }
        public bool IsCost          { get; set; }
        public bool IsFiscal        { get; set; }

        // Właściwości pomocnicze dla widoku
        public bool HasDescription  => !string.IsNullOrWhiteSpace(Description);
    }
}
