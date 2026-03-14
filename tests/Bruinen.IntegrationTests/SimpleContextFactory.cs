using Bruinen.Data;
using Microsoft.EntityFrameworkCore;

namespace Bruinen.IntegrationTests;

public class SimpleContextFactory(DbContextOptions<BruinenContext> options)
    : IDbContextFactory<BruinenContext>
{
    public BruinenContext CreateDbContext() => new(options);
}