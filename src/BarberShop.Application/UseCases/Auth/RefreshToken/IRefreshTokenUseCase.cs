using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.Auth.RefreshToken
{
    public interface IRefreshTokenUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(string refreshToken);
    }
}
