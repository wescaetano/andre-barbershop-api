using BarberShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Infra.Mappings
{
    public class ScheduleBlockMap : IEntityTypeConfiguration<ScheduleBlock>
    {
        public void Configure(EntityTypeBuilder<ScheduleBlock> builder)
        {
            builder.ToTable("ScheduleBlocks");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Reason).HasMaxLength(255);
            builder.HasOne(b => b.Barber)
                .WithMany()
                .HasForeignKey(b => b.BarberId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
