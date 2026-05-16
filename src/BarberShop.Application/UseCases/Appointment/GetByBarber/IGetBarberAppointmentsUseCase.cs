using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.Appointment.GetByBarber
{
    public interface IGetBarberAppointmentsUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long barberId, DateTime from, DateTime to);
    }
}
