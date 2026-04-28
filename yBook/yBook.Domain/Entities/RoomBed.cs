namespace yBook.Domain.Entities;

public class RoomBed
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string? DateModified { get; set; }
    public int RoomId { get; set; }
    public int Type { get; set; }
}
