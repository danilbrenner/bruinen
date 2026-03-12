using Bruinen.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bruinen.Data.Configurations;

public class RequestCounterConfiguration : IEntityTypeConfiguration<RequestCounter>
{
    public void Configure(EntityTypeBuilder<RequestCounter> builder)
    {
        builder.ToTable("request_counters").HasKey(rc => rc.Key);
        builder.Property(rc => rc.Key).HasMaxLength(255).IsRequired();
        builder.Property(rc => rc.Count).IsRequired().HasDefaultValue(0);
        builder.Property(rc => rc.LastUpdated).IsRequired();
        
        builder.HasIndex(rc => rc.Key);
    }
}

