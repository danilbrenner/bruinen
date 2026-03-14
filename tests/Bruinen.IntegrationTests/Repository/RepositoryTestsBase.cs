using Bruinen.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Bruinen.IntegrationTests.Repository;

public abstract class RepositoryTestsBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer Db = new PostgreSqlBuilder()
        .WithImage(Constants.Images.Postgres)
        .Build();
    
    protected IDbContextFactory<BruinenContext> ContextFactory = null!;
    
    public async Task InitializeAsync()
    {
        await Db.StartAsync();

        var options = new DbContextOptionsBuilder<BruinenContext>()
            .UseNpgsql(Db.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        ContextFactory = new SimpleContextFactory(options);

        await using var context = await ContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await Db.DisposeAsync();
}