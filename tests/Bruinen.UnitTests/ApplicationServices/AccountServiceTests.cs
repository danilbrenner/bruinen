using Bruinen.Application.Abstractions;
using Bruinen.Application.Services;
using Bruinen.Domain;
using Isopoh.Cryptography.Argon2;

namespace Bruinen.UnitTests.ApplicationServices;

public class AccountServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    [Theory]
    [AutoData]
    public async Task ChangePassword_WithValidCurrentPassword_ReturnsTrue(string login, string currentPassword, string newPassword)
    {
        // Arrange
        var passwordHash = Argon2.Hash(currentPassword);
        var user = new User(login, passwordHash, DateTimeOffset.UtcNow);

        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);

        var service = new AccountService(_userRepositoryMock.Object);

        // Act
        var result = await service.ChangePassword(login, currentPassword, newPassword);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [AutoData]
    public async Task ChangePassword_WithValidCurrentPassword_UpdatesUserPassword(string login, string currentPassword, string newPassword)
    {
        // Arrange
        var passwordHash = Argon2.Hash(currentPassword);
        var originalPasswordChangedAt = DateTimeOffset.UtcNow.AddDays(-10);
        var user = new User(login, passwordHash, originalPasswordChangedAt);

        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);

        var service = new AccountService(_userRepositoryMock.Object);

        // Act
        var beforeChangeHash = user.PasswordHash;
        var beforeChangeDate = user.PasswordChangedAt;
        await service.ChangePassword(login, currentPassword, newPassword);

        // Assert
        Assert.NotEqual(beforeChangeHash, user.PasswordHash);
        Assert.True(Argon2.Verify(user.PasswordHash, newPassword));
        Assert.True(user.PasswordChangedAt > beforeChangeDate);
    }

    [Theory]
    [AutoData]
    public async Task ChangePassword_WithInvalidCurrentPassword_ReturnsFalse(string login, string currentPassword, string wrongPassword, string newPassword)
    {
        // Arrange
        var passwordHash = Argon2.Hash(currentPassword);
        var user = new User(login, passwordHash, DateTimeOffset.UtcNow);

        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);

        var service = new AccountService(_userRepositoryMock.Object);

        // Act
        var result = await service.ChangePassword(login, wrongPassword, newPassword);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [AutoData]
    public async Task ChangePassword_WithInvalidCurrentPassword_DoesNotUpdatePassword(string login, string currentPassword, string wrongPassword, string newPassword)
    {
        // Arrange
        var passwordHash = Argon2.Hash(currentPassword);
        var user = new User(login, passwordHash, DateTimeOffset.UtcNow);

        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);

        var service = new AccountService(_userRepositoryMock.Object);

        // Act
        var originalHash = user.PasswordHash;
        var originalChangedAt = user.PasswordChangedAt;
        await service.ChangePassword(login, wrongPassword, newPassword);

        // Assert
        Assert.Equal(originalHash, user.PasswordHash);
        Assert.Equal(originalChangedAt, user.PasswordChangedAt);
    }

    [Theory]
    [AutoData]
    public async Task ChangePassword_WithNonExistentUser_ThrowsInvalidOperationException(string login, string currentPassword, string newPassword)
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync((User?)null);

        var service = new AccountService(_userRepositoryMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ChangePassword(login, currentPassword, newPassword));
        
        Assert.Equal("User not found.", exception.Message);
    }

    [Theory]
    [AutoData]
    public async Task ChangePassword_CallsRepositoryWithCorrectLogin(string login, string currentPassword, string newPassword)
    {
        // Arrange
        var passwordHash = Argon2.Hash(currentPassword);
        var user = new User(login, passwordHash, DateTimeOffset.UtcNow);

        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);

        var service = new AccountService(_userRepositoryMock.Object);

        // Act
        await service.ChangePassword(login, currentPassword, newPassword);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByLoginAsync(login), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task ChangePassword_WithSamePassword_ReturnsTrue(string login, string password)
    {
        // Arrange
        var passwordHash = Argon2.Hash(password);
        var user = new User(login, passwordHash, DateTimeOffset.UtcNow);

        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);

        var service = new AccountService(_userRepositoryMock.Object);

        // Act
        var result = await service.ChangePassword(login, password, password);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [AutoData]
    public async Task ChangePassword_UpdatesPasswordChangedAtTimestamp(string login, string currentPassword, string newPassword)
    {
        // Arrange
        var passwordHash = Argon2.Hash(currentPassword);
        var oldTimestamp = DateTimeOffset.UtcNow.AddDays(-30);
        var user = new User(login, passwordHash, oldTimestamp);

        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);

        var service = new AccountService(_userRepositoryMock.Object);
        var beforeChange = DateTimeOffset.UtcNow;

        // Act
        await service.ChangePassword(login, currentPassword, newPassword);

        // Assert
        Assert.True(user.PasswordChangedAt >= beforeChange);
        Assert.True(user.PasswordChangedAt > oldTimestamp);
    }
}

