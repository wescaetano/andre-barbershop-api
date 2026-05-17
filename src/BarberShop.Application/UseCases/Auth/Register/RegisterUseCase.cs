using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Utils;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BarberShop.Application.UseCases.Auth.Register
{
    public class RegisterUseCase : IRegisterUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;
        private readonly IBaseRepository<Profile> _profileRepository;
        private readonly long _defaultClientProfileId;

        public RegisterUseCase(
            IBaseRepository<Domain.User> userRepository,
            IBaseRepository<Profile> profileRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _defaultClientProfileId = configuration.GetValue<long>("DefaultClientProfileId");
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return FactoryResponse<dynamic>.InvalidModel("O campo 'name' é obrigatório.");

            if (string.IsNullOrWhiteSpace(model.Email))
                return FactoryResponse<dynamic>.InvalidModel("O campo 'email' é obrigatório.");

            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
                return FactoryResponse<dynamic>.InvalidModel("A senha deve ter no mínimo 6 caracteres.");

            if (_defaultClientProfileId == 0)
                return FactoryResponse<dynamic>.BadRequestErroInterno("Perfil padrão de cliente não configurado.");

            var existing = await _userRepository.Get(u => u.Email.ToLower() == model.Email.ToLower());
            if (existing != null)
                return FactoryResponse<dynamic>.Conflict("Já existe uma conta com este e-mail.");

            var profile = await _profileRepository.Get(p => p.Id == _defaultClientProfileId);
            if (profile == null)
                return FactoryResponse<dynamic>.NotFound("Perfil padrão de cliente não encontrado.");

            var user = new Domain.User
            {
                Name = model.Name.Trim(),
                Email = model.Email.Trim().ToLower(),
                Password = HashHelper.HashGeneration(model.Password),
            };
            user.AddCreationDate();
            user.ProfilesUsers.Add(new ProfileUser { ProfileId = _defaultClientProfileId });

            try
            {
                await _userRepository.Create(user);
                return FactoryResponse<dynamic>.SuccessfulCreation("Conta criada com sucesso!");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
