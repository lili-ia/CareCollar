using CareCollar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareCollar.Application.Contracts;

public interface ICareCollarDbContext
{
    DbSet<User> Users { get; set; }
    
    DbSet<Pet> Pets { get; set; }
    
    DbSet<CollarDevice> CollarDevices { get; set; }
    
    DbSet<HealthThreshold> HealthThresholds { get; set;}
    
    public DbSet<HealthData> HealthData { get; set; }
    
    DbSet<Notification> Notifications { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}