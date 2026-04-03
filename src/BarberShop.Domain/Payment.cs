using BarberShop.Communication.Enums.Payment;

namespace BarberShop.Domain
{
    public class Payment : BaseEntity
    {
        public long AppointmentId { get; set; }
        public string ExternalReference { get; set; } = null!;
        public decimal Amount { get; set; }
        public EPaymentStatus Status { get; set; } = EPaymentStatus.Pending;

        public Appointment Appointment { get; set; } = null!;
    }
}
