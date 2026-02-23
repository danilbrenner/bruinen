namespace Bruinen.Domain;

public class User(string login, string passwordHash, DateTime passwordChangedAt)
{
    public string Login { get; } = login;
    public string PasswordHash { get; private set; } = passwordHash;
    public DateTime PasswordChangedAt { get; private set; } = passwordChangedAt;

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordChangedAt = DateTime.UtcNow;
    }
}
