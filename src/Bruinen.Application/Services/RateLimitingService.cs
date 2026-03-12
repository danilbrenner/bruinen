using Bruinen.Application.Abstractions;

namespace Bruinen.Application.Services;

public class RateLimitingService(IRequestCounterRepository requestCounterRepository)
{
    public async Task<DateTimeOffset?> VerifyRequestAllowed(
        string key,
        int maxRequests,
        TimeSpan lockoutDuration)
    {
        var counter = await requestCounterRepository.Get(key);
        var lockoutExpiry = counter.LastUpdated.Add(lockoutDuration);

        if (counter.Count > 0 && counter.Count % maxRequests == 0 && lockoutExpiry > DateTimeOffset.UtcNow)
        {
            return lockoutExpiry;
        }

        counter.Increment();
        await requestCounterRepository.Save(counter);

        return null;
    }
}