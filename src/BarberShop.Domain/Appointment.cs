using BarberShop.Communication.Enums.Appointment;

namespace BarberShop.Domain
{
    public class Appointment : BaseEntity
    {
        public long UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EAppointmentStatus Status { get; set; } = EAppointmentStatus.WaitingPayment;

        public User User { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}
