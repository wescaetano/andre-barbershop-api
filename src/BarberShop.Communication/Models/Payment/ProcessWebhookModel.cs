namespace BarberShop.Communication.Models.Payment
{
    public class ProcessWebhookModel
    {
        public string Type { get; set; } = null!;
        public WebhookData Data { get; set; } = null!;

        public class WebhookData
        {
            public string Id { get; set; } = null!;
        }
    }
}
