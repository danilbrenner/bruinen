using Bruinen.Domain;

namespace Bruinen.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login);
}