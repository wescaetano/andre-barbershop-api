namespace BarberShop.Communication.Models.Appointment
{
    public class CreateAppointmentModel
    {
        public long UserId { get; set; }
        public long BarberId { get; set; }
        public long ServiceId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
    }
}
