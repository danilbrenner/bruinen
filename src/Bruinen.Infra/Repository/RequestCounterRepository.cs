using Bruinen.Application.Abstractions;
using Bruinen.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bruinen.Data.Repository;

public class RequestCounterRepository(IDbContextFactory<BruinenContext> contextFactory) : IRequestCounterRepository
{
    public async Task<RequestCounter> Get(string key)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var counterEntity = await context.RequestCounters.FirstOrDefaultAsync(rc => rc.Key == key);
        return counterEntity?.ToDomain() ?? new RequestCounter(key, DateTime.UtcNow);
    }

    public async Task Save(RequestCounter counter)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var counterEntity = await context.RequestCounters.FirstOrDefaultAsync(rc => rc.Key == counter.Key);
        
        if (counterEntity is null)
        {
            context.RequestCounters.Add(new Model.RequestCounter
            {
                Key = counter.Key,
                Count = counter.Count,
                LastUpdated = counter.LastUpdated
            });
        }
        else
        {
            counterEntity.Count = counter.Count;
            counterEntity.LastUpdated = counter.LastUpdated;
            
        }

        await context.SaveChangesAsync();
    }
}

