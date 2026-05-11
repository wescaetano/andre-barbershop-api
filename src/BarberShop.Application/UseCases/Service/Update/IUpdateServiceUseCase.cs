using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;

namespace BarberShop.Application.UseCases.Service.Update
{
    public interface IUpdateServiceUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(UpdateServiceModel model);
    }
}
