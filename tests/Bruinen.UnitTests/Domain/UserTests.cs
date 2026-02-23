using Bruinen.Domain;

namespace Bruinen.UnitTests.Domain;

public class UserTests
{
    [Theory]
    [AutoData]
    public void ChangePassword_ShouldUpdatePasswordHashAndChangedAt(
        string login, 
        string originalPasswordHash, 
        DateTimeOffset passwordChangedAt, 
        string newPasswordHash,
        DateTimeOffset newChangedAt)
    {
        // Arrange
        var user = new User(login, originalPasswordHash, passwordChangedAt);

        // Act
        user.ChangePassword(newPasswordHash, newChangedAt);

        // Assert
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.Equal(newChangedAt, user.PasswordChangedAt);
    }
}

