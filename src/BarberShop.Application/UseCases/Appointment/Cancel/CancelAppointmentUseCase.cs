using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.Cancel
{
    public class CancelAppointmentUseCase : ICancelAppointmentUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;

        public CancelAppointmentUseCase(IBaseRepository<Domain.Appointment> appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CancelAppointmentModel model)
        {
            var validator = new CancelAppointmentValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var appointment = await _appointmentRepository.Get(a => a.Id == model.AppointmentId);
            if (appointment == null)
                return FactoryResponse<dynamic>.NotFound("Agendamento não encontrado.");

            if (appointment.UserId != model.UserId)
                return FactoryResponse<dynamic>.Forbiden("Você não tem permissão para cancelar este agendamento.");

            if (appointment.Status != EAppointmentStatus.WaitingPayment)
                return FactoryResponse<dynamic>.BadRequest("Apenas agendamentos aguardando pagamento podem ser cancelados.");

            appointment.Status = EAppointmentStatus.Cancelled;
            appointment.AddUpdateDate();

            try
            {
                await _appointmentRepository.Update(appointment);
                return FactoryResponse<dynamic>.Success("Agendamento cancelado com sucesso.");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
