using yBook.Application.Ports;

namespace yBook.Application.UseCases;

public class LoginUserUseCase(IAuthRepository authRepository)
{
    public Task<bool> ExecuteAsync(string email, string password)
        => authRepository.LoginAsync(email, password);
}
