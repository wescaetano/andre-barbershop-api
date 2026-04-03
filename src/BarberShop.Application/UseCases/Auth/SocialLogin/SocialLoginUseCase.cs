using BarberShop.Application.Interfaces;
using BarberShop.Communication.Enums.User;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Utils;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Auth.SocialLogin
{
    public class SocialLoginUseCase : ISocialLoginUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;
        private readonly ITokenService _tokenService;

        public SocialLoginUseCase(
            IBaseRepository<Domain.User> userRepository,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(SocialLoginModel model)
        {
            var validator = new SocialLoginValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var user = await _userRepository.Get(u => u.Email.ToLower() == model.Email.ToLower());

            if (user != null)
            {
                var providerHash = HashHelper.HashGeneration(model.ProviderId);
                if (user.ProviderId != providerHash)
                {
                    user.ProviderId = providerHash;
                    await _userRepository.Update(user);
                }

                if (user.Status == EUserStatus.Inativo)
                    return FactoryResponse<dynamic>.BadRequest("Usuário inativo.");
            }
            else
            {
                user = new Domain.User
                {
                    Name = model.Name,
                    Email = model.Email,
                    ProviderId = HashHelper.HashGeneration(model.ProviderId),
                    Status = EUserStatus.Ativo
                };
                user.AddCreationDate();
                user.ProfilesUsers.Add(new ProfileUser { ProfileId = 2 });

                await _userRepository.Create(user);
            }

            return await _tokenService.GetToken(user);
        }
    }
}
