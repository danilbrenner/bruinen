namespace Bruinen.Data.Model;

public class RequestCounter
{
    public required string Key { get; init; }
    public int Count { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

