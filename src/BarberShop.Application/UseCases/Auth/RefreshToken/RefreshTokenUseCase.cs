using BarberShop.Application.Interfaces;
using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.Auth.RefreshToken
{
    public class RefreshTokenUseCase : IRefreshTokenUseCase
    {
        private readonly ITokenService _tokenService;

        public RefreshTokenUseCase(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return FactoryResponse<dynamic>.BadRequest("O refresh token não pode ser vazio.");

            var result = await _tokenService.ValidateRefreshToken(refreshToken);

            if (result == null)
                return FactoryResponse<dynamic>.Unauthorized("Refresh token inválido ou expirado.");

            return result;
        }
    }
}
