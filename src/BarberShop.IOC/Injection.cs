using BarberShop.Application.Config;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Application.UseCases.Appointment.Cancel;
using BarberShop.Application.UseCases.Appointment.Create;
using BarberShop.Application.UseCases.Appointment.GetAvailableSlots;
using BarberShop.Application.UseCases.Auth.Login;
using BarberShop.Application.UseCases.Auth.ResetPassword;
using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Application.UseCases.Auth.SocialLogin;
using BarberShop.Application.UseCases.Payment.Create;
using BarberShop.Application.UseCases.Payment.GetHistory;
using BarberShop.Application.UseCases.Payment.ProcessWebhook;
using BarberShop.Application.UseCases.User.ChangeStatus;
using BarberShop.Application.UseCases.User.Create;
using BarberShop.Application.UseCases.User.Update;
using BarberShop.Infra.Interfaces;
using BarberShop.Infra.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MoneyScope.Application.Services;
using MoneyScope.Infra.Repositories;

namespace BarberShop.IOC
{
    public static class Injection
    {
        public static IServiceCollection InjectDependencies(this IServiceCollection services, MigrationConfig? migrationConfig)
        {
            // Repositories
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped(typeof(IBaseRelationRepository<>), typeof(BaseRelationRepository<>));
            services.AddScoped<IRepositoryFactory, RepositoryFactory>();

            // Support services
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IBlobService, BlobService>();
            services.AddScoped<IAuthService, AuthService>();

            // Auth use cases
            services.AddScoped<ILoginUseCase, LoginUseCase>();
            services.AddScoped<ISocialLoginUseCase, SocialLoginUseCase>();
            services.AddScoped<ISendEmailResetPasswordUseCase, SendEmailResetPasswordUseCase>();
            services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();

            // User use cases
            services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
            services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();
            services.AddScoped<IChangeUserStatusUseCase, ChangeUserStatusUseCase>();

            // Appointment use cases
            services.AddScoped<ICreateAppointmentUseCase, CreateAppointmentUseCase>();
            services.AddScoped<IGetAvailableSlotsUseCase, GetAvailableSlotsUseCase>();
            services.AddScoped<ICancelAppointmentUseCase, CancelAppointmentUseCase>();

            // Payment use cases
            services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
            services.AddScoped<IProcessWebhookUseCase, ProcessWebhookUseCase>();
            services.AddScoped<IGetPaymentHistoryUseCase, GetPaymentHistoryUseCase>();

            return services;
        }
    }
}
