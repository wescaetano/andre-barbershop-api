using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Service.Update
{
    public class UpdateServiceUseCase : IUpdateServiceUseCase
    {
        private readonly IBaseRepository<Domain.Service> _repo;
        public UpdateServiceUseCase(IBaseRepository<Domain.Service> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(UpdateServiceModel model)
        {
            var service = await _repo.Get(model.Id);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");
            if (string.IsNullOrWhiteSpace(model.Name))
                return FactoryResponse<dynamic>.InvalidModel("Nome é obrigatório.");

            service.Name = model.Name.Trim();
            service.DurationMinutes = model.DurationMinutes;
            service.Price = model.Price;
            service.AddUpdateDate();
            await _repo.Update(service);
            return FactoryResponse<dynamic>.Success(new { service.Id, service.Name, service.DurationMinutes, service.Price });
        }
    }
}
