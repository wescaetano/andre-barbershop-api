using BarberShop.Application.Models.User;
using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.User.ChangeStatus
{
    public interface IChangeUserStatusUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(ChangeUserStatusModel model);
    }
}
