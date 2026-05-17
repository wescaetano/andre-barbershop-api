using BarberShop.Communication.Models;
using BarberShop.Communication.Models.WorkingHours;
namespace BarberShop.Application.UseCases.WorkingHours.Upsert
{
    public interface IUpsertWorkingHoursUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(UpsertWorkingHoursModel model);
    }
}
