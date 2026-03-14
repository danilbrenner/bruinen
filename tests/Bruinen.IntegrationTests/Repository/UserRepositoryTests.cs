using Bruinen.Data.Repository;
using Bruinen.Domain;
using UserEntity = Bruinen.Data.Model.User;

namespace Bruinen.IntegrationTests.Repository;

public class UserRepositoryTests : RepositoryTestsBase
{
    
    private async Task SeedUser(string login, string passwordHash, DateTime passwordChangedAt)
    {
        await using var context = await ContextFactory.CreateDbContextAsync();
        context.Users.Add(new UserEntity
        {
            Login = login,
            PasswordHash = passwordHash,
            PasswordChangedAt = passwordChangedAt
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByLogin_WhenUserDoesNotExist_ReturnsNull()
    {
        var sut = new UserRepository(ContextFactory);
        var result = await sut.GetByLogin("nobody");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByLogin_WhenUserExists_ReturnsUser()
    {
        await SeedUser("alice", "hash123", DateTime.UtcNow);
        var sut = new UserRepository(ContextFactory);

        var result = await sut.GetByLogin("alice");

        Assert.NotNull(result);
        Assert.Equal("alice", result.Login);
        Assert.Equal("hash123", result.PasswordHash);
    }

    [Fact]
    public async Task GetByLogin_IsCaseSensitive()
    {
        await SeedUser("Bob", "hash456", DateTime.UtcNow);
        var sut = new UserRepository(ContextFactory);
        
        var result = await sut.GetByLogin("bob");

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_WhenUserExists_UpdatesPasswordHashAndChangedAt()
    {
        var originalChangedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await SeedUser("charlie", "old-hash", originalChangedAt);
        var sut = new UserRepository(ContextFactory);
        
        var updatedAt = new DateTimeOffset(2026, 3, 14, 12, 0, 0, TimeSpan.Zero);
        var user = new User("charlie", "new-hash", updatedAt);
        await sut.Update(user);

        var result = await sut.GetByLogin("charlie");
        Assert.NotNull(result);
        Assert.Equal("new-hash", result.PasswordHash);
        Assert.Equal(updatedAt, result.PasswordChangedAt);
    }

    [Fact]
    public async Task Update_WhenUserDoesNotExist_ThrowsInvalidOperationException()
    {
        var sut = new UserRepository(ContextFactory);
        var user = new User("ghost", "hash", DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Update(user));
    }

    [Fact]
    public async Task Update_DoesNotAffectOtherUsers()
    {
        await SeedUser("dave", "dave-hash", DateTime.UtcNow);
        await SeedUser("eve", "eve-hash", DateTime.UtcNow);
        var sut = new UserRepository(ContextFactory);

        var dave = new User("dave", "dave-new-hash", DateTimeOffset.UtcNow);
        await sut.Update(dave);

        var eve = await sut.GetByLogin("eve");
        Assert.NotNull(eve);
        Assert.Equal("eve-hash", eve.PasswordHash);
    }
}

