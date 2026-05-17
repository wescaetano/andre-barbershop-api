using BarberShop.Application.Interfaces;
using BarberShop.Application.Models.User;
using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Communication.Models;
using BarberShop.Communication.Utils;
using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.User.Create
{
    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;
        private readonly IBaseRepository<Profile> _profileRepository;
        private readonly IBlobService _blobService;
        private readonly ISendEmailResetPasswordUseCase _sendEmailUseCase;

        public CreateUserUseCase(
            IBaseRepository<Domain.User> userRepository,
            IBaseRepository<Profile> profileRepository,
            IBlobService blobService,
            ISendEmailResetPasswordUseCase sendEmailUseCase)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _blobService = blobService;
            _sendEmailUseCase = sendEmailUseCase;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateUserModel model)
        {
            var validator = new CreateUserValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var userExists = await _userRepository.Get(u => u.Email.ToLower() == model.Email.ToLower());
            if (userExists != null)
                return FactoryResponse<dynamic>.Conflict("Já existe um usuário cadastrado com este email.");

            var profileExists = await _profileRepository.Get(p => p.Id == model.AccessProfile);
            if (profileExists == null)
                return FactoryResponse<dynamic>.NotFound("Perfil de acesso não encontrado.");

            var user = new Domain.User
            {
                Name = model.Name,
                Email = model.Email,
                Password = HashHelper.HashGeneration(model.Password),
                ProfilesUsers = new List<ProfileUser>() { new ProfileUser { ProfileId = model.AccessProfile } }
            };
            user.AddCreationDate();

            if (!string.IsNullOrWhiteSpace(model.ImageBase64))
            {
                var fileName = $"{Guid.NewGuid()}_{model.Name.Replace(" ", "_")}.png";
                user.ImageUrl = await _blobService.UploadBase64Async(model.ImageBase64, fileName);
            }

            try
            {
                await _userRepository.Create(user);
                return FactoryResponse<dynamic>.SuccessfulCreation("Usuário cadastrado com sucesso.");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
