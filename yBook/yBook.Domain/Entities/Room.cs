namespace yBook.Domain.Entities;

public class Room
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int PropertyId { get; set; }
    public string? DateModified { get; set; }
    public string? Name { get; set; }
    public int Type { get; set; }
    public bool IsAvailable { get; set; }
    public int MaxPeople { get; set; }
    public string? Area { get; set; }
    public string? Description { get; set; }
    public string? ShortName { get; set; }
    public int DefaultPrice { get; set; }
    public string? Color { get; set; }
    public string? Standard { get; set; }
    public int MinPeople { get; set; }
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
}
