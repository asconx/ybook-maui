using yBook.Application.Ports;

namespace yBook.Services;

public class AuthRepositoryAdapter(IAuthService authService) : IAuthRepository
{
    public yBook.Domain.Entities.UserSession? CurrentUser
    {
        get
        {
            var current = authService.CurrentUser;
            if (current == null)
            {
                return null;
            }

            return new yBook.Domain.Entities.UserSession
            {
                Token = current.Token,
                OrganizationId = current.OrganizationId,
                Role = current.Role,
                Permissions = current.Permissions
            };
        }
    }

    public Task<string?> GetTokenAsync() => authService.GetTokenAsync();

    public Task<bool> IsAuthenticatedAsync() => authService.IsAuthenticatedAsync();

    public Task<bool> LoginAsync(string email, string password) => authService.LoginAsync(email, password);

    public Task LogoutAsync() => authService.LogoutAsync();
}
