using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;

namespace BarberShop.Application.UseCases.Appointment.GetAvailableSlots
{
    public interface IGetAvailableSlotsUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(GetAvailableSlotsModel model);
    }
}
