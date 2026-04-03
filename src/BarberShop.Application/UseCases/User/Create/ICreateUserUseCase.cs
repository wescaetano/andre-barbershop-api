using BarberShop.Application.Models.User;
using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.User.Create
{
    public interface ICreateUserUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreateUserModel model);
    }
}
