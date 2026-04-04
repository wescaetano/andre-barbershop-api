using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Application.UseCases.User.GetById
{
    public class GetUserByIdUseCase : IGetUserByIdUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;

        public GetUserByIdUseCase(IBaseRepository<Domain.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long id)
        {
            if (id <= 0)
                return FactoryResponse<dynamic>.BadRequest("O campo 'id' deve ser maior que zero.");

            var user = await _userRepository.GetWithInclude(
                filter: u => u.Id == id,
                setIncludes: q => q
                    .Include(u => u.ProfilesUsers)
                    .ThenInclude(pu => pu.Profile)
            );

            if (user == null)
                return FactoryResponse<dynamic>.NotFound("Usuário não encontrado.");

            return FactoryResponse<dynamic>.Success(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.ImageUrl,
                Status = user.Status.ToString(),
                user.CreationDate,
                user.UpdateDate,
                Profiles = user.ProfilesUsers.Select(pu => new
                {
                    pu.ProfileId,
                    Name = pu.Profile?.Name
                })
            });
        }
    }
}
