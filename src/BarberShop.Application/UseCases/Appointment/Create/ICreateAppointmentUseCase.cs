using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;

namespace BarberShop.Application.UseCases.Appointment.Create
{
    public interface ICreateAppointmentUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreateAppointmentModel model);
    }
}
