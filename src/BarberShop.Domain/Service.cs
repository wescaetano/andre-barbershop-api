namespace BarberShop.Domain
{
    public class Service : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
