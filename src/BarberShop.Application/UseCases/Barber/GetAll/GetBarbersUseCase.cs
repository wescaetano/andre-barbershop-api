using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Barber.GetAll
{
    public class GetBarbersUseCase : IGetBarbersUseCase
    {
        private readonly IBaseRepository<Domain.Barber> _repo;
        public GetBarbersUseCase(IBaseRepository<Domain.Barber> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(bool activeOnly)
        {
            var barbers = activeOnly
                ? await _repo.GetAll(b => b.IsActive)
                : await _repo.Get();

            var result = barbers.Select(b => new
            {
                b.Id, b.UserId, b.DisplayName, b.IsActive
            }).ToList();

            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
