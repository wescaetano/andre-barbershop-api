using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Application.UseCases.Appointment.GetById
{
    public class GetAppointmentByIdUseCase : IGetAppointmentByIdUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;

        public GetAppointmentByIdUseCase(IBaseRepository<Domain.Appointment> appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long id)
        {
            if (id <= 0)
                return FactoryResponse<dynamic>.BadRequest("O campo 'id' deve ser maior que zero.");

            var appointment = await _appointmentRepository.GetWithInclude(
                filter: a => a.Id == id,
                setIncludes: q => q
                    .Include(a => a.User)
                    .Include(a => a.Payment)
            );

            if (appointment == null)
                return FactoryResponse<dynamic>.NotFound("Agendamento não encontrado.");

            return FactoryResponse<dynamic>.Success(new
            {
                appointment.Id,
                StartTime = appointment.StartTime.ToString("yyyy-MM-dd HH:mm"),
                EndTime   = appointment.EndTime.ToString("yyyy-MM-dd HH:mm"),
                Status    = appointment.Status.ToString(),
                appointment.CreationDate,
                User = new
                {
                    appointment.User.Id,
                    appointment.User.Name,
                    appointment.User.Email
                },
                Payment = appointment.Payment == null ? null : new
                {
                    appointment.Payment.Id,
                    appointment.Payment.Amount,
                    appointment.Payment.ExternalReference,
                    Status = appointment.Payment.Status.ToString()
                }
            });
        }
    }
}
