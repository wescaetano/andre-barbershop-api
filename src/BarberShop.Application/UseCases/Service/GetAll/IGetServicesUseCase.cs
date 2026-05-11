using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.Service.GetAll
{
    public interface IGetServicesUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(bool activeOnly);
    }
}
