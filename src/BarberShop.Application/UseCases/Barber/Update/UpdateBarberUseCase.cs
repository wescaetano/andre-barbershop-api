using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Barber.Update
{
    public class UpdateBarberUseCase : IUpdateBarberUseCase
    {
        private readonly IBaseRepository<Domain.Barber> _repo;
        public UpdateBarberUseCase(IBaseRepository<Domain.Barber> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(UpdateBarberModel model)
        {
            var barber = await _repo.Get(model.Id);
            if (barber == null)
                return FactoryResponse<dynamic>.NotFound("Barbeiro não encontrado.");
            if (string.IsNullOrWhiteSpace(model.DisplayName))
                return FactoryResponse<dynamic>.InvalidModel("Nome de exibição é obrigatório.");

            barber.DisplayName = model.DisplayName.Trim();
            barber.AddUpdateDate();
            await _repo.Update(barber);
            return FactoryResponse<dynamic>.Success(new { barber.Id, barber.DisplayName });
        }
    }
}
