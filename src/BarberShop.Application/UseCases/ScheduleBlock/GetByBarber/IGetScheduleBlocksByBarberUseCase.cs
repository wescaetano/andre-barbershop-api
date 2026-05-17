using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.ScheduleBlock.GetByBarber
{
    public interface IGetScheduleBlocksByBarberUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long barberId, DateTime from, DateTime to);
    }
}
