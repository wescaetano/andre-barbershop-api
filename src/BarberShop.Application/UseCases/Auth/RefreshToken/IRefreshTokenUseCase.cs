using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;

namespace BarberShop.Application.UseCases.Auth.RefreshToken
{
    public interface IRefreshTokenUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(string refreshToken);
    }
}
