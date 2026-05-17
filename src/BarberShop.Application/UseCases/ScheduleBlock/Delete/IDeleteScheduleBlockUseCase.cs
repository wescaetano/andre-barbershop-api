using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.ScheduleBlock.Delete
{
    public interface IDeleteScheduleBlockUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long id);
    }
}
