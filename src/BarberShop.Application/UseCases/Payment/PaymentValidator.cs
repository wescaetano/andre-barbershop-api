using BarberShop.Communication.Models.Payment;
using FluentValidation;

namespace BarberShop.Application.UseCases.Payment
{
    public class CreatePaymentValidator : AbstractValidator<CreatePaymentModel>
    {
        public CreatePaymentValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0).WithMessage("O campo 'agendamento' é obrigatório.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("O valor do pagamento deve ser maior que zero.");

            RuleFor(x => x.ServiceTitle)
                .NotEmpty().WithMessage("O campo 'título do serviço' é obrigatório.");

            RuleFor(x => x.NotificationUrl)
                .NotEmpty().WithMessage("O campo 'url de notificação' é obrigatório.");
        }
    }

    public class ProcessWebhookValidator : AbstractValidator<ProcessWebhookModel>
    {
        public ProcessWebhookValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("O campo 'type' é obrigatório.");

            RuleFor(x => x.Data)
                .NotNull().WithMessage("O campo 'data' é obrigatório.");

            When(x => x.Data != null, () =>
            {
                RuleFor(x => x.Data.Id)
                    .NotEmpty().WithMessage("O campo 'data.id' é obrigatório.");
            });
        }
    }
}
