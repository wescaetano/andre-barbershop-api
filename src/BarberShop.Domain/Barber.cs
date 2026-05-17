namespace BarberShop.Domain
{
    public class Barber : BaseEntity
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public User User { get; set; } = null!;
    }
}
