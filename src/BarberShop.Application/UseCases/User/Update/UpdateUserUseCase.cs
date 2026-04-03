using BarberShop.Application.Models.User;
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.User.Update
{
    public class UpdateUserUseCase : IUpdateUserUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;

        public UpdateUserUseCase(IBaseRepository<Domain.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(UpdateUserModel model)
        {
            var validator = new UpdateUserValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var user = await _userRepository.Get(u => u.Id == model.Id);
            if (user == null)
                return FactoryResponse<dynamic>.NotFound("Usuário não encontrado.");

            if (!string.IsNullOrWhiteSpace(model.Name)) user.Name = model.Name!;
            if (!string.IsNullOrWhiteSpace(model.Email)) user.Email = model.Email!;
            if (!string.IsNullOrWhiteSpace(model.ImageUrl)) user.ImageUrl = model.ImageUrl;
            user.AddUpdateDate();

            try
            {
                await _userRepository.Update(user);
                return FactoryResponse<dynamic>.Success("Usuário atualizado com sucesso.");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
