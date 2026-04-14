namespace yBook.Domain.Entities;

public class UserSession
{
    public string Token { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
