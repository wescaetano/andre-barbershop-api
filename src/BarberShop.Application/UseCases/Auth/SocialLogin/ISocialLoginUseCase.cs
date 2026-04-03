using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;

namespace BarberShop.Application.UseCases.Auth.SocialLogin
{
    public interface ISocialLoginUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(SocialLoginModel model);
    }
}
