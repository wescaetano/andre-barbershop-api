using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.WorkingHours.GetByBarber
{
    public class GetWorkingHoursByBarberUseCase : IGetWorkingHoursByBarberUseCase
    {
        private readonly IBaseRepository<Domain.WorkingHours> _repo;
        public GetWorkingHoursByBarberUseCase(IBaseRepository<Domain.WorkingHours> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long barberId)
        {
            var wh = await _repo.GetAll(w => w.BarberId == barberId);
            var result = wh.OrderBy(w => w.DayOfWeek).Select(w => new
            {
                w.Id, w.DayOfWeek, w.IsOpen,
                OpenTime = w.OpenTime.ToString("HH:mm"),
                CloseTime = w.CloseTime.ToString("HH:mm")
            }).ToList();
            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
