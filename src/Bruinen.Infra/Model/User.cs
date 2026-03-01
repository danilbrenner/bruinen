namespace Bruinen.Data.Model;

public class User
{
    public required string Login { get; init; }
    public required string PasswordHash { get; set; }
    public DateTime PasswordChangedAt { get; set; }
}
