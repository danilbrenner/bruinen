namespace Bruinen.Domain;

public class User(string login, string passwordHash, DateTimeOffset passwordChangedAt)
{
    public string Login { get; } = login;
    public string PasswordHash { get; private set; } = passwordHash;
    public DateTimeOffset PasswordChangedAt { get; private set; } = passwordChangedAt;

    public void ChangePassword(string newPasswordHash, DateTimeOffset changedAt)
    {
        PasswordHash = newPasswordHash;
        PasswordChangedAt = changedAt;
    }
}
