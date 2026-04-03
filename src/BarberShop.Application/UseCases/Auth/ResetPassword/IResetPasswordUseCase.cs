using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;

namespace BarberShop.Application.UseCases.Auth.ResetPassword
{
    public interface IResetPasswordUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(ResetPasswordModel model);
    }
}
