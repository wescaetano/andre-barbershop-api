using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
namespace BarberShop.Application.UseCases.Barber.ChangeStatus
{
    public interface IChangeBarberStatusUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(ChangeBarberStatusModel model);
    }
}
