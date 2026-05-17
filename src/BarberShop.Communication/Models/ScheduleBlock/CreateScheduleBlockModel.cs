namespace BarberShop.Communication.Models.ScheduleBlock
{
    public class CreateScheduleBlockModel
    {
        public long BarberId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Reason { get; set; }
    }
}
