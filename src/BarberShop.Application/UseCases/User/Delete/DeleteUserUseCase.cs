using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.User.Delete
{
    public class DeleteUserUseCase : IDeleteUserUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;

        public DeleteUserUseCase(IBaseRepository<Domain.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long id)
        {
            if (id <= 0)
                return FactoryResponse<dynamic>.BadRequest("O campo 'id' deve ser maior que zero.");

            var user = await _userRepository.Get(id);
            if (user == null)
                return FactoryResponse<dynamic>.NotFound("Usuário não encontrado.");

            await _userRepository.Remove(user);

            return FactoryResponse<dynamic>.Success("Usuário removido com sucesso.");
        }
    }
}
