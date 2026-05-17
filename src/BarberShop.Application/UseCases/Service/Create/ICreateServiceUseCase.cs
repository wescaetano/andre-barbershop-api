using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;

namespace BarberShop.Application.UseCases.Service.Create
{
    public interface ICreateServiceUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreateServiceModel model);
    }
}
