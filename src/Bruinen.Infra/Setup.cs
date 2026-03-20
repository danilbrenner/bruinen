using Bruinen.Application.Abstractions;
using Bruinen.Data.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bruinen.Data;

public static class Setup
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextFactory<BruinenContext>(options =>
        {
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });

        return services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IRequestCounterRepository, RequestCounterRepository>();
    }

    public static IApplicationBuilder UseData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BruinenContext>>();
        using var context = dbContextFactory.CreateDbContext();
        context.Database.Migrate();

        return app;
    }
}