namespace Bruinen.Data;

public static class MappingExtensions
{
    public static Domain.User ToDomain(this Model.User user)
        => new (user.Login, user.PasswordHash, user.PasswordChangedAt);
    
    public static Domain.RequestCounter ToDomain(this Model.RequestCounter requestCounter)
        => new (requestCounter.Key, requestCounter.LastUpdated, requestCounter.Count);
}