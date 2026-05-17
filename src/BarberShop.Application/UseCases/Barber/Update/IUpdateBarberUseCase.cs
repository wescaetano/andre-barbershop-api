using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
namespace BarberShop.Application.UseCases.Barber.Update
{
    public interface IUpdateBarberUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(UpdateBarberModel model);
    }
}
