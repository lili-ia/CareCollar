using CareCollar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareCollar.Persistence.Configurations;

public class HealthThresholdConfiguration : IEntityTypeConfiguration<HealthThreshold>
{
    public void Configure(EntityTypeBuilder<HealthThreshold> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.MetricType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.ThresholdName)
            .IsRequired()
            .HasMaxLength(100);
    }
}