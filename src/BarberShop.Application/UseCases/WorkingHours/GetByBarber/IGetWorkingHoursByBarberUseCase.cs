using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.WorkingHours.GetByBarber
{
    public interface IGetWorkingHoursByBarberUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long barberId);
    }
}
