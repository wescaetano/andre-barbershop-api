using BarberShop.Application.Interfaces;
using BarberShop.Communication.Enums.User;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Utils;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Auth.ResetPassword
{
    public class ResetPasswordUseCase : IResetPasswordUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;
        private readonly ITokenService _tokenService;

        public ResetPasswordUseCase(
            IBaseRepository<Domain.User> userRepository,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(ResetPasswordModel model)
        {
            var validator = new ResetPasswordValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            if (!_tokenService.IsValidToken(model.Token))
                return FactoryResponse<dynamic>.Forbiden("Token inválido ou expirado.");

            var email = _tokenService.Getclaim("unique_name", model.Token);
            if (string.IsNullOrWhiteSpace(email))
                return FactoryResponse<dynamic>.Forbiden("Não foi possível obter as informações do token.");

            var user = await _userRepository.Get(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
                return FactoryResponse<dynamic>.NotFound("Usuário não encontrado.");

            user.Password = HashHelper.HashGeneration(model.NewPassword);
            user.Status = EUserStatus.Ativo;

            await _userRepository.Update(user);

            return FactoryResponse<dynamic>.Success("Senha redefinida com sucesso.");
        }
    }
}
