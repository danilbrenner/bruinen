namespace Bruinen.Domain;

public class RequestCounter(string key, DateTimeOffset lastUpdated, int initialCount = 0)
{
    public string Key { get; private set; } = key;
    public int Count { get; private set; } = initialCount;
    public DateTimeOffset LastUpdated { get; private set; } = lastUpdated;

    public void Increment()
    {
        Count++;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}