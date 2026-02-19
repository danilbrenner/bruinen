using Bruinen.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bruinen.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users").HasKey(u => u.Login);
        builder.Property(u => u.Login).HasMaxLength(255).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.PasswordChangedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasIndex(u => u.Login);
    }
}