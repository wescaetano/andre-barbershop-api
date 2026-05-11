namespace BarberShop.Communication.Models.Appointment
{
    public class GetAvailableSlotsModel
    {
        public DateOnly Date { get; set; }
        public long ServiceId { get; set; }
    }
}
