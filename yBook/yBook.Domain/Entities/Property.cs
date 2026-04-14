namespace yBook.Domain.Entities;

public class Property
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string? DateModified { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
}
