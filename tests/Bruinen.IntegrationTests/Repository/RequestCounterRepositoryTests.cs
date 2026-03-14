using Bruinen.Data;
using Bruinen.Data.Repository;
using Bruinen.Domain;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Bruinen.IntegrationTests.Repository;

public class RequestCounterRepositoryTests : RepositoryTestsBase
{
    [Fact]
    public async Task Get_WhenKeyDoesNotExist_ReturnsNewCounterWithZeroCount()
    {
        var sut = new RequestCounterRepository(ContextFactory);
        var result = await sut.Get("missing-key");

        Assert.Equal("missing-key", result.Key);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public async Task Get_WhenKeyExists_ReturnsPersistedCounter()
    {
        var sut = new RequestCounterRepository(ContextFactory);
        var counter = new RequestCounter("existing-key", DateTimeOffset.UtcNow);
        counter.Increment();
        await sut.Save(counter);

        var result = await sut.Get("existing-key");

        Assert.Equal("existing-key", result.Key);
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public async Task Save_WhenCounterIsNew_PersistsIt()
    {
        var sut = new RequestCounterRepository(ContextFactory);
        var counter = new RequestCounter("new-key", DateTimeOffset.UtcNow);
        counter.Increment();
        counter.Increment();

        await sut.Save(counter);

        var result = await sut.Get("new-key");
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Save_WhenCounterAlreadyExists_UpdatesIt()
    {
        var sut = new RequestCounterRepository(ContextFactory);
        var counter = new RequestCounter("update-key", DateTimeOffset.UtcNow);
        await sut.Save(counter);

        counter.Increment();
        counter.Increment();
        counter.Increment();
        await sut.Save(counter);

        var result = await sut.Get("update-key");
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Save_WhenCounterAlreadyExists_DoesNotCreateDuplicate()
    {
        var sut = new RequestCounterRepository(ContextFactory);
        var counter = new RequestCounter("unique-key", DateTimeOffset.UtcNow);
        await sut.Save(counter);
        await sut.Save(counter);

        await using var context = await ContextFactory.CreateDbContextAsync();
        var count = await context.RequestCounters.CountAsync(rc => rc.Key == "unique-key");
        Assert.Equal(1, count);
    }
}

