using BarberShop.Application.Interfaces;
using BarberShop.Application.Models.User;
using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Communication.Models;
using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.User.Create
{
    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;
        private readonly IBaseRelationRepository<ProfileUser> _profileUserRepository;
        private readonly IBlobService _blobService;
        private readonly ISendEmailResetPasswordUseCase _sendEmailUseCase;

        public CreateUserUseCase(
            IBaseRepository<Domain.User> userRepository,
            IBaseRelationRepository<ProfileUser> profileUserRepository,
            IBlobService blobService,
            ISendEmailResetPasswordUseCase sendEmailUseCase)
        {
            _userRepository = userRepository;
            _profileUserRepository = profileUserRepository;
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

            var profileExists = await _profileUserRepository.Get(pu => pu.ProfileId == model.AccessProfile);
            if (profileExists == null)
                return FactoryResponse<dynamic>.NotFound("Perfil de acesso não encontrado.");

            var user = new Domain.User
            {
                Name = model.Name,
                Email = model.Email
            };
            user.AddCreationDate();

            if (!string.IsNullOrWhiteSpace(model.ImageBase64))
            {
                var fileName = $"{Guid.NewGuid()}_{model.Name.Replace(" ", "_")}.png";
                user.ImageUrl = await _blobService.UploadBase64Async(model.ImageBase64, fileName);
            }

            user.ProfilesUsers.Add(new ProfileUser { ProfileId = model.AccessProfile });

            try
            {
                await _userRepository.Create(user);
                await _sendEmailUseCase.ExecuteAsync(model.Email);
                return FactoryResponse<dynamic>.SuccessfulCreation("Usuário cadastrado com sucesso.");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
