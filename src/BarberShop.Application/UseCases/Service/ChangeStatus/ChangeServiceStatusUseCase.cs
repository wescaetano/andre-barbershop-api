using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Service.ChangeStatus
{
    public class ChangeServiceStatusUseCase : IChangeServiceStatusUseCase
    {
        private readonly IBaseRepository<Domain.Service> _repo;
        public ChangeServiceStatusUseCase(IBaseRepository<Domain.Service> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(ChangeServiceStatusModel model)
        {
            var service = await _repo.Get(model.Id);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");

            service.IsActive = model.IsActive;
            service.AddUpdateDate();
            await _repo.Update(service);
            return FactoryResponse<dynamic>.Success(null);
        }
    }
}
