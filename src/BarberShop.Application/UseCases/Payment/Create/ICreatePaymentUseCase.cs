using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Payment;

namespace BarberShop.Application.UseCases.Payment.Create
{
    public interface ICreatePaymentUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreatePaymentModel model);
    }
}
