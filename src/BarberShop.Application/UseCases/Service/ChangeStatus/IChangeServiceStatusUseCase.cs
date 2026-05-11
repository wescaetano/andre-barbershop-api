using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;

namespace BarberShop.Application.UseCases.Service.ChangeStatus
{
    public interface IChangeServiceStatusUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(ChangeServiceStatusModel model);
    }
}
