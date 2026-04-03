using BarberShop.Application.Models.User;
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.User.ChangeStatus
{
    public class ChangeUserStatusUseCase : IChangeUserStatusUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;

        public ChangeUserStatusUseCase(IBaseRepository<Domain.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(ChangeUserStatusModel model)
        {
            var validator = new ChangeUserStatusValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var user = await _userRepository.Get(u => u.Id == model.Id);
            if (user == null)
                return FactoryResponse<dynamic>.NotFound("Usuário não encontrado.");

            user.Status = model.Status;
            user.AddUpdateDate();

            try
            {
                await _userRepository.Update(user);
                return FactoryResponse<dynamic>.Success("Status do usuário atualizado com sucesso.");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
