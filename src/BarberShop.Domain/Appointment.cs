using BarberShop.Communication.Enums.Appointment;

namespace BarberShop.Domain
{
    public class Appointment : BaseEntity
    {
        public long UserId { get; set; }
        public long? BarberId { get; set; }   // nullable for backward compat
        public long? ServiceId { get; set; }  // nullable for backward compat
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EAppointmentStatus Status { get; set; } = EAppointmentStatus.WaitingPayment;

        public User User { get; set; } = null!;
        public Barber? Barber { get; set; }
        public Service? Service { get; set; }
        public Payment? Payment { get; set; }
    }
}
