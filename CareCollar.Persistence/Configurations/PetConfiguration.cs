using CareCollar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareCollar.Persistence.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.Species) 
            .HasConversion<string>() 
            .IsRequired()
            .HasMaxLength(50); 
        
        builder.HasMany(p => p.Thresholds)
            .WithOne(t => t.Pet)
            .HasForeignKey(t => t.PetId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(p => p.Devices)
            .WithOne(c => c.Pet)
            .HasForeignKey(c => c.PetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}