using Bruinen.Application.Abstractions;
using Bruinen.Application.Services;
using Bruinen.Domain;
using Isopoh.Cryptography.Argon2;
using Moq;

namespace Bruinen.UnitTests.ApplicationServices;

public class LoginServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    
    [Theory]
    [AutoData]
    public async Task LoginAsync_WithValidCredentials_ReturnsTrue(string login, string password)
    {
        // Arrange
        var passwordHash = Argon2.Hash(password);
        var user = new User(login, passwordHash, DateTimeOffset.UtcNow);

        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);
        
        var service = new LoginService(_userRepositoryMock.Object);

        // Act
        var result = await service.LoginAsync(login, password);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [AutoData]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFalse(string login, string password, string wrongPassword)
    {
        // Arrange
        var passwordHash = Argon2.Hash(password);
        var user = new User(login, passwordHash, DateTimeOffset.UtcNow);
        
        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync(user);
        
        var service = new LoginService(_userRepositoryMock.Object);

        // Act
        var result = await service.LoginAsync(login, wrongPassword);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [AutoData]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFalse(string login, string password)
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetByLoginAsync(login))
            .ReturnsAsync((User?)null);
        
        var service = new LoginService(_userRepositoryMock.Object);

        // Act
        var result = await service.LoginAsync(login, password);

        // Assert
        Assert.False(result);
    }
}

