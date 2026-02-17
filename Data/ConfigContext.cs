using Microsoft.EntityFrameworkCore;
using ConfigService.Models;

namespace ConfigService.Data;

public class ConfigContext : DbContext
{
    public ConfigContext(DbContextOptions<ConfigContext> options) : base(options) { }

    public DbSet<Application> Applications { get; set; }
    public DbSet<Models.Environment> Environments { get; set; }
    public DbSet<ConfigItem> ConfigItems { get; set; }
    public DbSet<SystemOption> SystemOptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>()
            .HasMany(a => a.Environments)
            .WithOne(e => e.Application)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Models.Environment>()
            .HasMany(e => e.ConfigItems)
            .WithOne(c => c.Environment)

            .HasForeignKey(c => c.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<ConfigItem>()
            .HasIndex(c => new { c.EnvironmentId, c.Key })
            .IsUnique();
    }
}
