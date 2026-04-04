using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Payment;

namespace BarberShop.Application.UseCases.Payment.ProcessWebhook
{
    public interface IProcessWebhookUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(ProcessWebhookModel model);
    }
}
