using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Application.UseCases.Appointment.GetByUser
{
    public class GetUserAppointmentsUseCase : IGetUserAppointmentsUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;

        public GetUserAppointmentsUseCase(IBaseRepository<Domain.Appointment> appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long userId)
        {
            if (userId <= 0)
                return FactoryResponse<dynamic>.BadRequest("O campo 'userId' deve ser maior que zero.");

            var query = _appointmentRepository.GetAllWithInclude(
                filter: a => a.UserId == userId,
                setIncludes: q => q.Include(a => a.Payment)
            );

            var appointments = await Task.FromResult(
                query
                    .OrderByDescending(a => a.StartTime)
                    .Select(a => new
                    {
                        a.Id,
                        StartTime = a.StartTime.ToString("yyyy-MM-dd HH:mm"),
                        EndTime = a.EndTime.ToString("yyyy-MM-dd HH:mm"),
                        Status = (int)a.Status,
                        a.CreationDate,
                        Payment = a.Payment == null ? null : new
                        {
                            a.Payment.Id,
                            a.Payment.Amount,
                            a.Payment.ExternalReference,
                            Status = (int)a.Payment.Status
                        }
                    })
                    .ToList<object>()
            );

            return FactoryResponse<dynamic>.Success(appointments);
        }
    }
}
