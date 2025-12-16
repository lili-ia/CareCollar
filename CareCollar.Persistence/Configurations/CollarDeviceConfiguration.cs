using CareCollar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareCollar.Persistence.Configurations;

public class CollarDeviceConfiguration : IEntityTypeConfiguration<CollarDevice>
{
    public void Configure(EntityTypeBuilder<CollarDevice> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.SerialNumber)
            .IsUnique();

        builder.Property(c => c.SerialNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasMany<HealthData>()
            .WithOne(h => h.CollarDevice)
            .HasForeignKey(h => h.CollarId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}