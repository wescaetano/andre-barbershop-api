using BarberShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Infra.Mappings
{
    public class WorkingHoursMap : IEntityTypeConfiguration<WorkingHours>
    {
        public void Configure(EntityTypeBuilder<WorkingHours> builder)
        {
            builder.ToTable("WorkingHours");
            builder.HasKey(w => w.Id);
            builder.Property(w => w.OpenTime).HasColumnType("time(6)");
            builder.Property(w => w.CloseTime).HasColumnType("time(6)");
            builder.HasOne(w => w.Barber)
                .WithMany()
                .HasForeignKey(w => w.BarberId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(w => new { w.BarberId, w.DayOfWeek }).IsUnique();
        }
    }
}
