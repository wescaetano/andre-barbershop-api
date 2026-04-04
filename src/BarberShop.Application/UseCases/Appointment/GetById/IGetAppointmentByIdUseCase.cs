using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.Appointment.GetById
{
    public interface IGetAppointmentByIdUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long id);
    }
}
