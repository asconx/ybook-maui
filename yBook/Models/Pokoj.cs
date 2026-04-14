namespace yBook.Models
{
    public class Pokoj
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int PropertyId { get; set; }
        public string? DateModified { get; set; }
        public string? Nazwa { get; set; }
        public int Type { get; set; }
        public bool CzyDostepny { get; set; }
        public int MaxOsobLiczbą { get; set; }
        public string? Powierzchnia { get; set; }
        public string? Opis { get; set; }
        public string? ShortName { get; set; }
        public int DefaultPrice { get; set; }
        public string? Kolor { get; set; }
        public string? Standard { get; set; }
        public int MinOsobLiczbą { get; set; }
        public int LockId { get; set; }
        public int CalendarPosition { get; set; }
        public string? ImageUrl { get; set; }
        public int? PhotoFileId { get; set; }
        public string? BedSummary { get; set; }
        public string? AmenitySummary { get; set; }
        public List<string> BedItems { get; set; } = [];
        public List<string> AmenityItems { get; set; } = [];
        public string? PropertyName { get; set; }
        public string? PropertyAddress { get; set; }
        public int? PriceModifierId { get; set; }

        public string StatusStr => CzyDostepny ? "✅ Dostępny" : "❌ Niedostępny";
        public string DetailStr => $"{Powierzchnia}m² • {MaxOsobLiczbą} osób";
        public string AvailabilityText => CzyDostepny ? "Tak" : "Nie";
        public string NazwaText => string.IsNullOrWhiteSpace(Nazwa) ? "-" : Nazwa!;
        public string ShortNameText => string.IsNullOrWhiteSpace(ShortName) ? "-" : ShortName!;
        public string AreaText => string.IsNullOrWhiteSpace(Powierzchnia) ? "-" : Powierzchnia!;
        public string ColorText => string.IsNullOrWhiteSpace(Kolor) ? "-" : Kolor!;
        public string ColorValue => string.IsNullOrWhiteSpace(Kolor) ? "#FFFFFF" : Kolor!;
        public string BedSummaryText => string.IsNullOrWhiteSpace(BedSummary) ? "Brak danych o łóżkach" : BedSummary!;
        public string AmenitySummaryText => string.IsNullOrWhiteSpace(AmenitySummary) ? "Brak udogodnień" : AmenitySummary!;
        public string PropertyNameText => string.IsNullOrWhiteSpace(PropertyName) ? "-" : PropertyName!;
        public string PropertyAddressText => string.IsNullOrWhiteSpace(PropertyAddress) ? "-" : PropertyAddress!;
        public string PriceModifierText => PriceModifierId?.ToString() ?? "-";
        public string PhotoFileIdText => PhotoFileId?.ToString() ?? "-";
        public string MinMaxPeopleText => $"{MinOsobLiczbą} / {MaxOsobLiczbą}";
        public string DescriptionText => string.IsNullOrWhiteSpace(Opis) ? "-" : Opis!;
        public string StandardText => string.IsNullOrWhiteSpace(Standard) ? "-" : Standard!;
        public string DateModifiedText => string.IsNullOrWhiteSpace(DateModified) ? "-" : DateModified!;
    }
}
