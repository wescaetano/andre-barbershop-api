using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Token;
using BarberShop.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface ITokenService
    {
        Task<ResponseModel<dynamic>?> ValidateRefreshToken(string token);
        Task<string> GenerateRefreshToken(long userId);
        Task<ResponseModel<dynamic>> GenerateToken(GenerateTokenModel model);
        Task<ResponseModel<dynamic>> ExcludeExpiredRefreshTokens();
        Task<ResponseModel<dynamic>> GetToken(User user, int? seconds = null);
        Task<ResponseModel<dynamic>> GenerateTokenByEmail(string email);
        bool IsValidToken(string token);
        string? Getclaim(string claim, string token);
    }
}
