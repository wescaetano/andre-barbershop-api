using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BarberShop.Application.UseCases.Auth.Register
{
    public class RegisterUseCase : IRegisterUseCase
    {
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<Profile> _profileRepository;
        private readonly ISendEmailResetPasswordUseCase _sendEmailUseCase;
        private readonly long _defaultClientProfileId;

        public RegisterUseCase(
            IBaseRepository<User> userRepository,
            IBaseRepository<Profile> profileRepository,
            ISendEmailResetPasswordUseCase sendEmailUseCase,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _sendEmailUseCase = sendEmailUseCase;
            _defaultClientProfileId = configuration.GetValue<long>("DefaultClientProfileId");
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return FactoryResponse<dynamic>.InvalidModel("O campo 'name' é obrigatório.");

            if (string.IsNullOrWhiteSpace(model.Email))
                return FactoryResponse<dynamic>.InvalidModel("O campo 'email' é obrigatório.");

            if (_defaultClientProfileId == 0)
                return FactoryResponse<dynamic>.BadRequestErroInterno("Perfil padrão de cliente não configurado.");

            var existing = await _userRepository.Get(u => u.Email.ToLower() == model.Email.ToLower());
            if (existing != null)
                return FactoryResponse<dynamic>.Conflict("Já existe uma conta com este e-mail.");

            var profile = await _profileRepository.Get(p => p.Id == _defaultClientProfileId);
            if (profile == null)
                return FactoryResponse<dynamic>.NotFound("Perfil padrão de cliente não encontrado.");

            var user = new User { Name = model.Name, Email = model.Email };
            user.AddCreationDate();
            user.ProfilesUsers.Add(new ProfileUser { ProfileId = _defaultClientProfileId });

            try
            {
                await _userRepository.Create(user);
                await _sendEmailUseCase.ExecuteAsync(model.Email);
                return FactoryResponse<dynamic>.SuccessfulCreation("Conta criada! Verifique seu e-mail para definir sua senha.");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
