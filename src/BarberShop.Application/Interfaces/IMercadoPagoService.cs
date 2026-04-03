namespace BarberShop.Application.Interfaces
{
    public interface IMercadoPagoService
    {
        Task<string> CreatePreferenceAsync(string title, decimal amount, string externalReference, string notificationUrl);
        Task<(string status, string externalReference, decimal amount)> GetPaymentStatusAsync(string paymentId);
    }
}
