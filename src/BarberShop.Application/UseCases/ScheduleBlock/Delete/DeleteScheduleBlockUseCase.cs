using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.ScheduleBlock.Delete
{
    public class DeleteScheduleBlockUseCase : IDeleteScheduleBlockUseCase
    {
        private readonly IBaseRepository<Domain.ScheduleBlock> _repo;
        public DeleteScheduleBlockUseCase(IBaseRepository<Domain.ScheduleBlock> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long id)
        {
            var block = await _repo.Get(id);
            if (block == null)
                return FactoryResponse<dynamic>.NotFound("Bloqueio não encontrado.");

            await _repo.Remove(block);
            return FactoryResponse<dynamic>.Success(null);
        }
    }
}
