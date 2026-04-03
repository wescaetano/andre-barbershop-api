using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Models.Token;
using FluentValidation;

namespace BarberShop.Application.UseCases.Auth
{
    public class LoginValidator : AbstractValidator<LoginModel>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("O campo 'login' é obrigatório.")
                .EmailAddress().WithMessage("O campo 'login' deve ser um email válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("O campo 'senha' é obrigatório.");
        }
    }

    public class SocialLoginValidator : AbstractValidator<SocialLoginModel>
    {
        public SocialLoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O campo 'email' é obrigatório.")
                .EmailAddress().WithMessage("O campo 'email' deve ser um email válido.");

            RuleFor(x => x.ProviderId)
                .NotEmpty().WithMessage("O campo 'providerId' é obrigatório.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O campo 'nome' é obrigatório.");
        }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordModel>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("O token é obrigatório.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("A nova senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");
        }
    }
}
