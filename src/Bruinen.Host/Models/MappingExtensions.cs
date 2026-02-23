using Bruinen.Domain;

namespace Bruinen.Host.Models;

public static class MappingExtensions
{
    public  static AccountViewModel ToViewModel(this User user)
        => new ()
        {
            PasswordLastChangedAt = DateOnly.FromDateTime(user.PasswordChangedAt.Date)
        };
}