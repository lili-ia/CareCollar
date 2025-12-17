using CareCollar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareCollar.Application.Contracts;

public interface ICareCollarDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Pet> Pets { get; }
    
    DbSet<CollarDevice> CollarDevices { get; }
    
    DbSet<HealthThreshold> HealthThresholds { get; }
    
    public DbSet<HealthData> HealthData { get; set; }
    
    DbSet<Notification> Notifications { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}