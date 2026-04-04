using BarberShop.Application.Interfaces;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using BarberShop.Core.Enums.SendEmail;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Auth.SendEmailResetPassword
{
    public class SendEmailResetPasswordUseCase : ISendEmailResetPasswordUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public SendEmailResetPasswordUseCase(
            IBaseRepository<Domain.User> userRepository,
            ITokenService tokenService,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(string email)
        {
            var user = await _userRepository.Get(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
                return FactoryResponse<dynamic>.NotFound("Usuário não encontrado.");

            var tokenResult = await _tokenService.GenerateTokenByEmail(email);
            if (!tokenResult.Success)
                return FactoryResponse<dynamic>.BadRequest("Não foi possível gerar o token de redefinição.");

            var token = tokenResult.Data?.ToString() ?? string.Empty;

            var sendEmailModel = new SendEmailModel(
                subject: "Redefinição de senha",
                to: email,
                content: $"Use o token a seguir para redefinir sua senha: {token}");

            var sent = await _emailService.SendEmail(sendEmailModel, ERedefinitionEmailType.RequestToResetPassword, user.Name);
            if (!sent)
                return FactoryResponse<dynamic>.BadRequest("Não foi possível enviar o email de redefinição.");

            return FactoryResponse<dynamic>.Success("Email de redefinição de senha enviado com sucesso.");
        }
    }
}
