namespace BarberShop.Communication.Models.Payment
{
    public class CreatePaymentModel
    {
        public long AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string ServiceTitle { get; set; } = null!;
        public string NotificationUrl { get; set; } = null!;
    }
}
