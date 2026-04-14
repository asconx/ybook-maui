namespace yBook.Domain.Entities;

public class RoomPhoto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int RoomId { get; set; }
    public string? DateModified { get; set; }
    public int FileId { get; set; }
}
