namespace yBook.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<string?> GetTokenAsync();
        Task<bool> IsAuthenticatedAsync();
        UserSession? CurrentUser { get; }
    }

    public class UserSession
    {
        public string Token           { get; set; } = string.Empty;
        public int    OrganizationId  { get; set; }
        public string Role            { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = [];

        public bool HasPermission(string permission) =>
            Permissions.Contains(permission);
    }
}
