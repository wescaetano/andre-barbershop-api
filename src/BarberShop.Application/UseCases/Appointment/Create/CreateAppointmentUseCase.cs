using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.Create
{
    public class CreateAppointmentUseCase : ICreateAppointmentUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;
        private const int SlotMinutes = 30;

        public CreateAppointmentUseCase(IBaseRepository<Domain.Appointment> appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateAppointmentModel model)
        {
            var validator = new CreateAppointmentValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var startTime = model.Date.ToDateTime(model.StartTime);
            var endTime = startTime.AddMinutes(SlotMinutes);

            var slotOccupied = await _appointmentRepository.Get(
                a => a.StartTime == startTime && a.Status != EAppointmentStatus.Cancelled);

            if (slotOccupied != null)
                return FactoryResponse<dynamic>.Conflict("Este horário já está ocupado.");

            var appointment = new Domain.Appointment
            {
                UserId = model.UserId,
                StartTime = startTime,
                EndTime = endTime,
                Status = EAppointmentStatus.WaitingPayment
            };
            appointment.AddCreationDate();

            try
            {
                await _appointmentRepository.Create(appointment);
                return FactoryResponse<dynamic>.SuccessfulCreation(new
                {
                    appointment.Id,
                    appointment.StartTime,
                    appointment.EndTime,
                    Status = appointment.Status.ToString()
                });
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
