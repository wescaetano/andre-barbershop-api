using BarberShop.Application.Models.User;
using FluentValidation;

namespace BarberShop.Application.UseCases.User
{
    public class CreateUserValidator : AbstractValidator<CreateUserModel>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O campo 'nome' é obrigatório.")
                .MaximumLength(100).WithMessage("O campo 'nome' deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O campo 'email' é obrigatório.")
                .EmailAddress().WithMessage("O campo 'email' deve ser um endereço de email válido.");

            RuleFor(x => x.AccessProfile)
                .GreaterThan(0).WithMessage("O campo 'perfil de acesso' é obrigatório.");
        }
    }

    public class UpdateUserValidator : AbstractValidator<UpdateUserModel>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("O campo 'id' é obrigatório.");

            When(x => x.Email is not null, () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("O campo 'email' deve ser um endereço de email válido.");
            });

            When(x => x.Name is not null, () =>
            {
                RuleFor(x => x.Name)
                    .MaximumLength(100).WithMessage("O campo 'nome' deve ter no máximo 100 caracteres.");
            });
        }
    }

    public class ChangeUserStatusValidator : AbstractValidator<ChangeUserStatusModel>
    {
        public ChangeUserStatusValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("O campo 'id' é obrigatório.");
        }
    }
}
