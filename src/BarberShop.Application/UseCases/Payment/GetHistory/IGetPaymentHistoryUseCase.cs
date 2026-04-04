using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.Payment.GetHistory
{
    public interface IGetPaymentHistoryUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long userId);
    }
}
