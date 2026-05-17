using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
namespace BarberShop.Application.UseCases.Barber.Create
{
    public interface ICreateBarberUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreateBarberModel model);
    }
}
