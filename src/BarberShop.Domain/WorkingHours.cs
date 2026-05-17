namespace BarberShop.Domain
{
    public class WorkingHours : BaseEntity
    {
        public long BarberId { get; set; }
        public int DayOfWeek { get; set; }   // 0 = Sunday … 6 = Saturday
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
        public bool IsOpen { get; set; }

        public Barber Barber { get; set; } = null!;
    }
}
