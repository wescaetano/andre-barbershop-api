using BarberShop.Communication.Models;
using BarberShop.Communication.Models.ScheduleBlock;
namespace BarberShop.Application.UseCases.ScheduleBlock.Create
{
    public interface ICreateScheduleBlockUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreateScheduleBlockModel model);
    }
}
