using BarberShop.Application.Interfaces;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Enums.Payment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Payment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Payment.ProcessWebhook
{
    public class ProcessWebhookUseCase : IProcessWebhookUseCase
    {
        private readonly IBaseRepository<Domain.Payment> _paymentRepository;
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;
        private readonly IMercadoPagoService _mercadoPagoService;

        public ProcessWebhookUseCase(
            IBaseRepository<Domain.Payment> paymentRepository,
            IBaseRepository<Domain.Appointment> appointmentRepository,
            IMercadoPagoService mercadoPagoService)
        {
            _paymentRepository = paymentRepository;
            _appointmentRepository = appointmentRepository;
            _mercadoPagoService = mercadoPagoService;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(ProcessWebhookModel model)
        {
            var validator = new ProcessWebhookValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            if (model.Type != "payment")
                return FactoryResponse<dynamic>.Success("Evento ignorado.");

            try
            {
                var (status, externalReference, amount) = await _mercadoPagoService.GetPaymentStatusAsync(model.Data.Id);

                var payment = await _paymentRepository.Get(p => p.ExternalReference == externalReference);
                if (payment == null)
                    return FactoryResponse<dynamic>.NotFound("Pagamento não encontrado.");

                payment.Status = status switch
                {
                    "approved" => EPaymentStatus.Approved,
                    "rejected" => EPaymentStatus.Rejected,
                    _ => EPaymentStatus.Pending
                };
                payment.AddUpdateDate();
                await _paymentRepository.Update(payment);

                if (payment.Status == EPaymentStatus.Approved)
                {
                    var appointment = await _appointmentRepository.Get(a => a.Id == payment.AppointmentId);
                    if (appointment != null)
                    {
                        appointment.Status = EAppointmentStatus.Paid;
                        appointment.AddUpdateDate();
                        await _appointmentRepository.Update(appointment);
                    }
                }

                return FactoryResponse<dynamic>.Success("Webhook processado com sucesso.");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
