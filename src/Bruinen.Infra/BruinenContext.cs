using Bruinen.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Bruinen.Data;

public class BruinenContext(DbContextOptions<BruinenContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BruinenContext).Assembly);
    }
}