using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.Auth.SendEmailResetPassword
{
    public interface ISendEmailResetPasswordUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(string email);
    }
}
