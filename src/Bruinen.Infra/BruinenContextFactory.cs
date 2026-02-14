using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bruinen.Data;

public class BruinenContextFactory : IDesignTimeDbContextFactory<BruinenContext>
{
    public BruinenContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BruinenContext>();
        
        // Use a default connection string for design-time migrations
        // This can be overridden with --connection parameter when applying migrations
        optionsBuilder
            .UseNpgsql()
            .UseSnakeCaseNamingConvention();

        return new BruinenContext(optionsBuilder.Options);
    }
}

