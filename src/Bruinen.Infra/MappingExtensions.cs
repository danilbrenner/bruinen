namespace Bruinen.Data;

public static class MappingExtensions
{
    public static Domain.User ToDomain(this Model.User user)
        => new (user.Login, user.PasswordHash);
}