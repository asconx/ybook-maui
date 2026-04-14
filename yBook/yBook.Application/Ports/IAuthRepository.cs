using yBook.Domain.Entities;

namespace yBook.Application.Ports;

public interface IAuthRepository
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsAuthenticatedAsync();
    UserSession? CurrentUser { get; }
}
