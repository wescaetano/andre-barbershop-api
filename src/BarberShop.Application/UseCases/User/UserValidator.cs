using BarberShop.Application.Models.User;
using FluentValidation;

namespace BarberShop.Application.UseCases.User
{
    public class GetUsersPaginatedValidator : AbstractValidator<GetUsersPaginatedModel>
    {
        private static readonly string[] _allowedSortFields = ["Id", "Name", "Email", "CreationDate"];

        public GetUsersPaginatedValidator()
        {
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("O tamanho da página deve ser entre 1 e 100.");

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("O número da página deve ser maior ou igual a 1.");

            RuleFor(x => x.SortField)
                .Must(f => _allowedSortFields.Contains(f, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Campo de ordenação inválido. Valores permitidos: {string.Join(", ", _allowedSortFields)}.");

            RuleFor(x => x.SortOrder)
                .Must(o => string.Equals(o, "asc", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(o, "desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Direção de ordenação inválida. Valores permitidos: 'asc', 'desc'.");
        }
    }

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
