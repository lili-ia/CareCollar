using System.Reflection;
using CareCollar.Application.Contracts;
using CareCollar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareCollar.Persistence;

public class CareCollarDbContext(DbContextOptions<CareCollarDbContext> options) : DbContext(options), ICareCollarDbContext
{
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

    public DbSet<User> Users { get; } = null!;
    
    public DbSet<Pet> Pets { get; } = null!;
    
    public DbSet<CollarDevice> CollarDevices { get; } = null!;
    
    public DbSet<HealthThreshold> HealthThresholds { get; } = null!;
    
    public DbSet<HealthData> HealthData { get; set; } = null!;
}