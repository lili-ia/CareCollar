using CareCollar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareCollar.Persistence.Configurations;

public class HealthDataConfiguration : IEntityTypeConfiguration<HealthData>
{
    public void Configure(EntityTypeBuilder<HealthData> builder)
    {
        builder.HasKey(h => new { h.Time, h.CollarId });
        builder.HasIndex(h => h.Time);
        
        builder.Property(h => h.Time)
            .IsRequired();

        builder.Property(h => h.GpsLatitude).HasPrecision(10, 8);
        builder.Property(h => h.GpsLongitude).HasPrecision(10, 8);
        builder.Property(h => h.HeartRateBPM).HasPrecision(6, 2);
    }
}