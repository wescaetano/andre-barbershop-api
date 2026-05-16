using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.Create
{
    public class CreateAppointmentUseCase : ICreateAppointmentUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;
        private readonly IBaseRepository<Domain.Service> _serviceRepository;

        public CreateAppointmentUseCase(
            IBaseRepository<Domain.Appointment> appointmentRepository,
            IBaseRepository<Domain.Service> serviceRepository)
        {
            _appointmentRepository = appointmentRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateAppointmentModel model)
        {
            var service = await _serviceRepository.Get(model.ServiceId);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");

            var startTime = model.Date.ToDateTime(model.StartTime);
            var endTime = startTime.AddMinutes(service.DurationMinutes);

            var slotOccupied = await _appointmentRepository.Get(
                a => a.BarberId == model.BarberId
                  && a.StartTime < endTime
                  && a.EndTime > startTime
                  && a.Status != EAppointmentStatus.Cancelled);

            if (slotOccupied != null)
                return FactoryResponse<dynamic>.Conflict("Este horário já está ocupado.");

            var appointment = new Domain.Appointment
            {
                UserId = model.UserId,
                BarberId = model.BarberId,
                ServiceId = model.ServiceId,
                StartTime = startTime,
                EndTime = endTime,
                Status = EAppointmentStatus.WaitingPayment
            };
            appointment.AddCreationDate();

            await _appointmentRepository.Create(appointment);
            return FactoryResponse<dynamic>.SuccessfulCreation(new
            {
                appointment.Id,
                appointment.StartTime,
                appointment.EndTime,
                Status = appointment.Status.ToString()
            });
        }
    }
}
