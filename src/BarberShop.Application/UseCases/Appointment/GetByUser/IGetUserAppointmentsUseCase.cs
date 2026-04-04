using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.Appointment.GetByUser
{
    public interface IGetUserAppointmentsUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long userId);
    }
}
