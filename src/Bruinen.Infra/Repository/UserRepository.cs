using Bruinen.Application.Abstractions;
using Bruinen.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bruinen.Data.Repository;

public class UserRepository(IDbContextFactory<BruinenContext> contextFactory) : IUserRepository
{
    public async Task<User?> GetByLoginAsync(string login)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var userEntity = await context.Users.FirstOrDefaultAsync(u => u.Login == login);
        return userEntity?.ToDomain();
    }

    public async Task UpdateAsync(User user)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var userEntity = await context.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
        if (userEntity == null)
            throw new InvalidOperationException($"User with login '{user.Login}' not found.");
        
        userEntity.PasswordHash = user.PasswordHash;
        userEntity.PasswordChangedAt = user.PasswordChangedAt.UtcDateTime;
        
        await context.SaveChangesAsync();
    }
}