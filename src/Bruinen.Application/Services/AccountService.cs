using Bruinen.Application.Abstractions;
using Isopoh.Cryptography.Argon2;

namespace Bruinen.Application.Services;

public class AccountService(IUserRepository userRepository)
{
    public async Task<bool> ChangePassword(string login, string currentPassword, string newPassword)
    {
        var user = await userRepository.GetByLoginAsync(login);
        if(user == null)
            throw new InvalidOperationException("User not found.");
        if (!Argon2.Verify(user.PasswordHash, currentPassword))
            return false;
        user.ChangePassword(Argon2.Hash(newPassword), DateTimeOffset.UtcNow);
        await userRepository.UpdateAsync(user);
        return true;
    }
}