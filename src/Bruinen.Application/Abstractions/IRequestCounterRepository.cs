using Bruinen.Domain;

namespace Bruinen.Application.Abstractions;

public interface IRequestCounterRepository
{
    Task<RequestCounter> Get(string key);
    Task Save(RequestCounter counter);
}