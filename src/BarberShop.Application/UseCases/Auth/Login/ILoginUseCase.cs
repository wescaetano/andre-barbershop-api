using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Token;

namespace BarberShop.Application.UseCases.Auth.Login
{
    public interface ILoginUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(LoginModel model);
    }
}
