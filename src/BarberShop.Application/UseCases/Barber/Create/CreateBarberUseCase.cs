using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BarberShop.Application.UseCases.Barber.Create
{
    public class CreateBarberUseCase : ICreateBarberUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepo;
        private readonly IBaseRepository<Domain.Barber> _barberRepo;
        private readonly ISendEmailResetPasswordUseCase _sendEmail;
        private readonly long _barberProfileId;

        public CreateBarberUseCase(
            IBaseRepository<Domain.User> userRepo,
            IBaseRepository<Domain.Barber> barberRepo,
            ISendEmailResetPasswordUseCase sendEmail,
            IConfiguration configuration)
        {
            _userRepo = userRepo;
            _barberRepo = barberRepo;
            _sendEmail = sendEmail;
            _barberProfileId = configuration.GetValue<long>("DefaultBarberProfileId");
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateBarberModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return FactoryResponse<dynamic>.InvalidModel("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
                return FactoryResponse<dynamic>.InvalidModel("E-mail inválido.");
            if (string.IsNullOrWhiteSpace(model.DisplayName))
                return FactoryResponse<dynamic>.InvalidModel("Nome de exibição é obrigatório.");

            var email = model.Email.Trim().ToLower();
            var existing = await _userRepo.Get(u => u.Email.ToLower() == email);
            if (existing != null)
                return FactoryResponse<dynamic>.Conflict("Já existe uma conta com este e-mail.");

            var user = new Domain.User { Name = model.Name.Trim(), Email = email };
            user.AddCreationDate();
            user.ProfilesUsers.Add(new ProfileUser { ProfileId = _barberProfileId });

            await _userRepo.Create(user);

            var barber = new Domain.Barber
            {
                UserId = user.Id,
                DisplayName = model.DisplayName.Trim(),
                IsActive = true
            };
            barber.AddCreationDate();
            await _barberRepo.Create(barber);

            await _sendEmail.ExecuteAsync(email);
            return FactoryResponse<dynamic>.SuccessfulCreation(new { barber.Id, barber.DisplayName, user.Email });
        }
    }
}
