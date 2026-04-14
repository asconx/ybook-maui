namespace yBook.Domain.Entities;

public class RoomPriceModifier
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string? DateModified { get; set; }
    public int RoomId { get; set; }
    public int PriceModifierId { get; set; }
}
