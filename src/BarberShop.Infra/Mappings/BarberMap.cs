using BarberShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Infra.Mappings
{
    public class BarberMap : IEntityTypeConfiguration<Barber>
    {
        public void Configure(EntityTypeBuilder<Barber> builder)
        {
            builder.ToTable("Barbers");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.DisplayName).IsRequired().HasMaxLength(100);
            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(b => b.UserId).IsUnique();
        }
    }
}
