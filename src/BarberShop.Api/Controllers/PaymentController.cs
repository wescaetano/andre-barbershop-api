using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.Payment.Create;
using BarberShop.Application.UseCases.Payment.GetHistory;
using BarberShop.Application.UseCases.Payment.ProcessWebhook;
using BarberShop.Communication.Models.Payment;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Gerenciamento de pagamentos via Mercado Pago</summary>
    public class PaymentController : BaseController
    {
        private readonly ICreatePaymentUseCase _createPaymentUseCase;
        private readonly IProcessWebhookUseCase _processWebhookUseCase;
        private readonly IGetPaymentHistoryUseCase _getPaymentHistoryUseCase;

        /// <summary></summary>
        public PaymentController(
            ICreatePaymentUseCase createPaymentUseCase,
            IProcessWebhookUseCase processWebhookUseCase,
            IGetPaymentHistoryUseCase getPaymentHistoryUseCase)
        {
            _createPaymentUseCase = createPaymentUseCase;
            _processWebhookUseCase = processWebhookUseCase;
            _getPaymentHistoryUseCase = getPaymentHistoryUseCase;
        }

        /// <summary>Cria uma preferência de pagamento no Mercado Pago para um agendamento</summary>
        [HttpPost]
        [APIAuthorization("Payments-C", "Payments-E", "Payments-V", "Payments-I")]
        public async Task<IActionResult> Create([FromBody] CreatePaymentModel model)
        {
            var result = await _createPaymentUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Endpoint público chamado pelo Mercado Pago para notificar mudanças de status de pagamento</summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> ProcessWebhook([FromBody] ProcessWebhookModel model)
        {
            var result = await _processWebhookUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Retorna o histórico de pagamentos de um usuário</summary>
        [HttpGet("history/{userId:long}")]
        [APIAuthorization("Payments-C", "Payments-E", "Payments-V", "Payments-I")]
        public async Task<IActionResult> GetHistory([FromRoute] long userId)
        {
            var result = await _getPaymentHistoryUseCase.ExecuteAsync(userId);
            return Result(result);
        }
    }
}
