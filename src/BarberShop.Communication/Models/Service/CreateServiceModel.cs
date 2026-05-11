namespace BarberShop.Communication.Models.Service
{
    public class CreateServiceModel
    {
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
    }
}
