using System.Reflection;
using CareCollar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareCollar.Persistence;

public class CareCollarDbContext(DbContextOptions<CareCollarDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<Pet> Pets { get; set; } = null!;
    
    public DbSet<CollarDevice> CollarDevices { get; set; } = null!;
    
    public DbSet<HealthData> HealthData { get; set; } = null!;
    
    public DbSet<HealthThreshold> HealthThresholds { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(optionsBuilder);
    }
}