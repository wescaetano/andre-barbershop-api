using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.GetByBarber
{
    public class GetBarberAppointmentsUseCase : IGetBarberAppointmentsUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _repo;
        public GetBarberAppointmentsUseCase(IBaseRepository<Domain.Appointment> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long barberId, DateTime from, DateTime to)
        {
            var appointments = await _repo.GetAll(
                a => a.BarberId == barberId
                  && a.StartTime >= from
                  && a.StartTime < to);

            var result = appointments
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    a.BarberId,
                    a.ServiceId,
                    a.StartTime,
                    a.EndTime,
                    Status = (int)a.Status,
                    a.CreationDate
                }).ToList();

            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
