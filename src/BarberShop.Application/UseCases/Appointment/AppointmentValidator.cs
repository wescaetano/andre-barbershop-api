using BarberShop.Communication.Models.Appointment;
using FluentValidation;

namespace BarberShop.Application.UseCases.Appointment
{
    public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentModel>
    {
        public CreateAppointmentValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("O campo 'usuário' é obrigatório.");

            RuleFor(x => x.Date)
                .Must(d => d >= DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("A data do agendamento não pode ser no passado.");

            RuleFor(x => x.StartTime)
                .Must(t => t >= new TimeOnly(9, 0) && t <= new TimeOnly(18, 0))
                .WithMessage("O horário deve estar entre 09:00 e 18:00.");
        }
    }

    public class GetAvailableSlotsValidator : AbstractValidator<GetAvailableSlotsModel>
    {
        public GetAvailableSlotsValidator()
        {
            RuleFor(x => x.Date)
                .Must(d => d >= DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("A data não pode ser no passado.");

            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("O campo 'serviço' é obrigatório.");
        }
    }

    public class CancelAppointmentValidator : AbstractValidator<CancelAppointmentModel>
    {
        public CancelAppointmentValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0).WithMessage("O campo 'agendamento' é obrigatório.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("O campo 'usuário' é obrigatório.");
        }
    }
}
