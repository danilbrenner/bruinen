using Bruinen.Data.Model;
using Isopoh.Cryptography.Argon2;

namespace Bruinen.E2ETests;

public class TestUserFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BruinenContext>()
            .UseNpgsql(E2ESettings.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = new BruinenContext(options);

        var passwordHash = Argon2.Hash(E2ESettings.TestPassword);
        var existing = await context.Users.FindAsync(E2ESettings.TestUsername);

        if (existing is not null)
            existing.PasswordHash = passwordHash;
        else
            context.Users.Add(new User
            {
                Login = E2ESettings.TestUsername,
                PasswordHash = passwordHash
            });

        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition(Name)]
public class E2ECollection : ICollectionFixture<TestUserFixture>
{
    public const string Name = "E2E";
}
