using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Barber.ChangeStatus
{
    public class ChangeBarberStatusUseCase : IChangeBarberStatusUseCase
    {
        private readonly IBaseRepository<Domain.Barber> _repo;
        public ChangeBarberStatusUseCase(IBaseRepository<Domain.Barber> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(ChangeBarberStatusModel model)
        {
            var barber = await _repo.Get(model.Id);
            if (barber == null)
                return FactoryResponse<dynamic>.NotFound("Barbeiro não encontrado.");

            barber.IsActive = model.IsActive;
            barber.AddUpdateDate();
            await _repo.Update(barber);
            return FactoryResponse<dynamic>.Success(null);
        }
    }
}
