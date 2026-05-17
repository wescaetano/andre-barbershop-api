using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Service.GetAll
{
    public class GetServicesUseCase : IGetServicesUseCase
    {
        private readonly IBaseRepository<Domain.Service> _repo;
        public GetServicesUseCase(IBaseRepository<Domain.Service> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(bool activeOnly)
        {
            var services = activeOnly
                ? await _repo.GetAll(s => s.IsActive)
                : await _repo.Get();

            var result = services.Select(s => new
            {
                s.Id, s.Name, s.DurationMinutes, s.Price, s.IsActive
            }).ToList();

            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
