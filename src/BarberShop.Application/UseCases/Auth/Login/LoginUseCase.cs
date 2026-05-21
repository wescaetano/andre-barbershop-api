using BarberShop.Application.Interfaces;
using BarberShop.Communication.Enums.User;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Token;
using BarberShop.Communication.Utils;
using BarberShop.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BarberShop.Application.UseCases.Auth.Login
{
    public class LoginUseCase : ILoginUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginUseCase> _logger;

        public LoginUseCase(
            IBaseRepository<Domain.User> userRepository,
            ITokenService tokenService,
            ILogger<LoginUseCase> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _logger = logger;
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

            var loginNormalized = model.Login.Trim().ToLower();

            var user = await _userRepository.GetWithInclude(
                u => u.Email == loginNormalized,
                query => query.Include(u => u.ProfilesUsers).ThenInclude(up => up.Profile));

            if (user == null || string.IsNullOrWhiteSpace(user.Password))
            {
                _logger.LogWarning("Login failed: user not found for email {Email}", loginNormalized);
                return FactoryResponse<dynamic>.Unauthorized("Usuário não encontrado.");
            }

            var passwordMatch = HashHelper.PasswordCompare(user.Password, model.Password);
            if (!passwordMatch)
            {
                _logger.LogWarning("Login failed: password mismatch for user {UserId}", user.Id);
                return FactoryResponse<dynamic>.Unauthorized("Senha inválida.");
            }

            if (user.Status == EUserStatus.Inativo)
                return FactoryResponse<dynamic>.BadRequest("Usuário inativo.");

            return await _tokenService.GetToken(user);
        }
    }
}
