using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.ScheduleBlock.GetByBarber
{
    public class GetScheduleBlocksByBarberUseCase : IGetScheduleBlocksByBarberUseCase
    {
        private readonly IBaseRepository<Domain.ScheduleBlock> _repo;
        public GetScheduleBlocksByBarberUseCase(IBaseRepository<Domain.ScheduleBlock> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long barberId, DateTime from, DateTime to)
        {
            var blocks = await _repo.GetAll(
                b => b.BarberId == barberId && b.EndTime > from && b.StartTime < to);

            var result = blocks.OrderBy(b => b.StartTime).Select(b => new
            {
                b.Id, b.BarberId, b.StartTime, b.EndTime, b.Reason
            }).ToList();

            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
