using BarberShop.Application.Models.User;
using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.User.Update
{
    public interface IUpdateUserUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(UpdateUserModel model);
    }
}
