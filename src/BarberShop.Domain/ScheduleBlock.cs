namespace BarberShop.Domain
{
    public class ScheduleBlock : BaseEntity
    {
        public long BarberId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Reason { get; set; }

        public Barber Barber { get; set; } = null!;
    }
}
