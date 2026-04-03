using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;

namespace BarberShop.Application.UseCases.Appointment.Cancel
{
    public interface ICancelAppointmentUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CancelAppointmentModel model);
    }
}
