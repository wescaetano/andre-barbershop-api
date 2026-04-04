using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Payment.GetHistory
{
    public class GetPaymentHistoryUseCase : IGetPaymentHistoryUseCase
    {
        private readonly IBaseRepository<Domain.Payment> _paymentRepository;
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;

        public GetPaymentHistoryUseCase(
            IBaseRepository<Domain.Payment> paymentRepository,
            IBaseRepository<Domain.Appointment> appointmentRepository)
        {
            _paymentRepository = paymentRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long userId)
        {
            if (userId <= 0)
                return FactoryResponse<dynamic>.InvalidModel("O campo 'usuário' é obrigatório.");

            var appointments = await _appointmentRepository.GetAll(a => a.UserId == userId);
            var appointmentIds = appointments.Select(a => a.Id).ToHashSet();

            var payments = await _paymentRepository.GetAll(p => appointmentIds.Contains(p.AppointmentId));

            var history = payments.Select(p =>
            {
                var appointment = appointments.First(a => a.Id == p.AppointmentId);
                return new
                {
                    Date = appointment.StartTime.ToString("yyyy-MM-dd"),
                    StartTime = appointment.StartTime.ToString("HH:mm"),
                    Amount = p.Amount,
                    Status = p.Status.ToString()
                };
            }).OrderByDescending(p => p.Date).ToList<object>();

            return FactoryResponse<dynamic>.Success(history);
        }
    }
}
