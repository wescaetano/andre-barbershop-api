using BarberShop.Application.Interfaces;
using BarberShop.Communication.Enums.User;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Token;
using BarberShop.Communication.Utils;
using BarberShop.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Application.UseCases.Auth.Login
{
    public class LoginUseCase : ILoginUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;
        private readonly ITokenService _tokenService;

        public LoginUseCase(
            IBaseRepository<Domain.User> userRepository,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(LoginModel model)
        {
            var validator = new LoginValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var user = await _userRepository.GetWithInclude(
                u => u.Email.ToLower() == model.Login.ToLower(),
                query => query.Include(u => u.ProfilesUsers).ThenInclude(up => up.Profile));

            if (user == null || string.IsNullOrWhiteSpace(user.Password))
                return FactoryResponse<dynamic>.Unauthorized("Credenciais inválidas.");

            if (!HashHelper.PasswordCompare(user.Password, model.Password))
                return FactoryResponse<dynamic>.Unauthorized("Credenciais inválidas.");

            if (user.Status == EUserStatus.Inativo)
                return FactoryResponse<dynamic>.BadRequest("Usuário inativo.");

            return await _tokenService.GetToken(user);
        }
    }
}
