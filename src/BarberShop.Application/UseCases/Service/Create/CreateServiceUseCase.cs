using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Service.Create
{
    public class CreateServiceUseCase : ICreateServiceUseCase
    {
        private readonly IBaseRepository<Domain.Service> _repo;
        public CreateServiceUseCase(IBaseRepository<Domain.Service> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateServiceModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return FactoryResponse<dynamic>.InvalidModel("Nome é obrigatório.");
            if (model.DurationMinutes <= 0)
                return FactoryResponse<dynamic>.InvalidModel("Duração deve ser maior que zero.");
            if (model.Price < 0)
                return FactoryResponse<dynamic>.InvalidModel("Preço não pode ser negativo.");

            var service = new Domain.Service
            {
                Name = model.Name.Trim(),
                DurationMinutes = model.DurationMinutes,
                Price = model.Price,
                IsActive = true
            };
            service.AddCreationDate();
            await _repo.Create(service);
            return FactoryResponse<dynamic>.SuccessfulCreation(new { service.Id, service.Name, service.DurationMinutes, service.Price });
        }
    }
}
