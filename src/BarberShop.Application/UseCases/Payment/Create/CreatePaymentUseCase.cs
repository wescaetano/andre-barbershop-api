using BarberShop.Application.Interfaces;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Enums.Payment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Payment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Payment.Create
{
    public class CreatePaymentUseCase : ICreatePaymentUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;
        private readonly IBaseRepository<Domain.Payment> _paymentRepository;
        private readonly IMercadoPagoService _mercadoPagoService;

        public CreatePaymentUseCase(
            IBaseRepository<Domain.Appointment> appointmentRepository,
            IBaseRepository<Domain.Payment> paymentRepository,
            IMercadoPagoService mercadoPagoService)
        {
            _appointmentRepository = appointmentRepository;
            _paymentRepository = paymentRepository;
            _mercadoPagoService = mercadoPagoService;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreatePaymentModel model)
        {
            var validator = new CreatePaymentValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var appointment = await _appointmentRepository.Get(a => a.Id == model.AppointmentId);
            if (appointment == null)
                return FactoryResponse<dynamic>.NotFound("Agendamento não encontrado.");

            if (appointment.Status != EAppointmentStatus.WaitingPayment)
                return FactoryResponse<dynamic>.BadRequest("Este agendamento não está aguardando pagamento.");

            var externalReference = $"appointment_{appointment.Id}";

            try
            {
                var checkoutUrl = await _mercadoPagoService.CreatePreferenceAsync(
                    model.ServiceTitle, model.Amount, externalReference, model.NotificationUrl);

                var payment = new Domain.Payment
                {
                    AppointmentId = appointment.Id,
                    ExternalReference = externalReference,
                    Amount = model.Amount,
                    Status = EPaymentStatus.Pending
                };
                payment.AddCreationDate();

                await _paymentRepository.Create(payment);

                return FactoryResponse<dynamic>.Success(new { checkoutUrl });
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
