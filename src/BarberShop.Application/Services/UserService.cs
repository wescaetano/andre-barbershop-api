using BarberShop.Application.Interfaces;
using BarberShop.Application.Models.User;
using BarberShop.Communication.Filters;
using BarberShop.Communication.Models;
using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;
using MoneyScope.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class UserService : BaseService, IUserService
    {
        private readonly IBlobService _blobService;
        private readonly IAuthService _authService;
        public UserService(IRepositoryFactory repositoryFactory, IBlobService blobService, IAuthService authService) : base(repositoryFactory)
        {
            _blobService = blobService;
            _authService = authService;
        }

        public async Task<ResponseModel<dynamic>> Create(CreateUserModel model)
        {
            var userExist = await _repository<User>().Get(u => u.Email.ToLower() == model.Email.ToLower());
            if (userExist != null) return FactoryResponse<dynamic>.Conflict("Já existe um usuário cadastrado com este email.");

            var user = new User
            {
                Email = model.Email,
                Name = model.Name
            };

            // Save the image to blob
            if (!string.IsNullOrWhiteSpace(model.ImageBase64))
            {
                var fileName = $"{Guid.NewGuid()}_{model.Name.Replace(" ", "_")}.png";
                var imageUrl = await _blobService.UploadBase64Async(model.ImageBase64, fileName);
                user.ImageUrl = imageUrl;
            }

            // Verifica se o perfil de acesso existe e retorna erro se não existir
            var userProfileExists = await _relationRepository<ProfileUser>().Get(pu => pu.ProfileId == model.AccessProfile);
            if (userProfileExists == null) return FactoryResponse<dynamic>.NotFound("Perfil de acesso não encontrado.");
            user.ProfilesUsers.Add(new ProfileUser {ProfileId = model.AccessProfile});


            try
            {
                await _repository<User>().Create(user);
                await _authService.SendEmailResetPassword(user.Email);
                return FactoryResponse<dynamic>.SuccessfulCreation("Usuário cadastrado com sucesso.");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno("Erro: " + e.Message);
            }
        }
        public async Task<ResponseModel<dynamic>> Update(UpdateUserModel model)
        {
            throw new NotImplementedException();
        }
        public async Task<ResponseModel<dynamic>> ChangeStatus(ChangeUserStatusModel model)
        {
            var user = await _repository<User>().Get(u => u.Id == model.Id);
            if (user == null) return FactoryResponse<dynamic>.NotFound("Usuário não encontrado!");

            user.Status = model.Status;

            try
            {
                await _repository<User>().Update(user);
                return FactoryResponse<dynamic>.Success("Usuário cadastrado com sucesso!");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno("Erro interno: " + e.Message);
            }
        }
        public async Task<ResponseModel<dynamic>> GetById(long id)
        {
            throw new NotImplementedException();
        }
        public async Task<ResponseModel<dynamic>> GetPaginated(UserFilterModel filter)
        {
            throw new NotImplementedException();
        }
    }
}
