using BarberShop.Application.Config;
using BarberShop.Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BarberShop.Application.Services
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly MercadoPagoConfig _config;
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "https://api.mercadopago.com";

        public MercadoPagoService(IOptions<MercadoPagoConfig> config, HttpClient httpClient)
        {
            _config = config.Value;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config.AccessToken);
        }

        public async Task<string> CreatePreferenceAsync(string title, decimal amount, string externalReference, string notificationUrl)
        {
            var payload = new
            {
                items = new[]
                {
                    new
                    {
                        title,
                        quantity = 1,
                        unit_price = amount,
                        currency_id = "BRL"
                    }
                },
                external_reference = externalReference,
                notification_url = notificationUrl
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/checkout/preferences", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            return doc.RootElement.GetProperty("init_point").GetString()
                ?? throw new InvalidOperationException("Mercado Pago did not return an init_point.");
        }

        public async Task<(string status, string externalReference, decimal amount)> GetPaymentStatusAsync(string paymentId)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/v1/payments/{paymentId}");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var status = root.GetProperty("status").GetString() ?? string.Empty;
            var externalReference = root.GetProperty("external_reference").GetString() ?? string.Empty;
            var transactionAmount = root.GetProperty("transaction_amount").GetDecimal();

            return (status, externalReference, transactionAmount);
        }
    }
}
