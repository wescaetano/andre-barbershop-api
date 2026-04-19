using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;

namespace BarberShop.Application.UseCases.Auth.Register
{
    public interface IRegisterUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(RegisterModel model);
    }
}
