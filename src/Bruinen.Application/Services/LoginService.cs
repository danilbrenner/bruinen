using Bruinen.Application.Abstractions;
using Isopoh.Cryptography.Argon2;

namespace Bruinen.Application.Services;

public class LoginService(IUserRepository userRepository)
{
    public async Task<bool> LoginAsync(string login, string password)
        => await userRepository.GetByLogin(login) is { } user && Argon2.Verify(user.PasswordHash, password);
}