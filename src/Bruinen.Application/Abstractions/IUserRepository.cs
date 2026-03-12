using Bruinen.Domain;

namespace Bruinen.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByLogin(string login);
    Task Update(User user);
}