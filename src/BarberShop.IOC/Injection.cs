using BarberShop.Application.Config;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Application.UseCases.Appointment.Cancel;
using BarberShop.Application.UseCases.Appointment.Create;
using BarberShop.Application.UseCases.Appointment.GetAvailableSlots;
using BarberShop.Application.UseCases.Appointment.GetById;
using BarberShop.Application.UseCases.Appointment.GetByUser;
using BarberShop.Application.UseCases.Auth.Login;
using BarberShop.Application.UseCases.Auth.RefreshToken;
using BarberShop.Application.UseCases.Auth.Register;
using BarberShop.Application.UseCases.Auth.ResetPassword;
using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Application.UseCases.Auth.SocialLogin;
using BarberShop.Application.UseCases.Payment.Create;
using BarberShop.Application.UseCases.Payment.GetHistory;
using BarberShop.Application.UseCases.Payment.ProcessWebhook;
using BarberShop.Application.UseCases.Barber.ChangeStatus;
using BarberShop.Application.UseCases.Barber.Create;
using BarberShop.Application.UseCases.Barber.GetAll;
using BarberShop.Application.UseCases.Barber.Update;
using BarberShop.Application.UseCases.Service.ChangeStatus;
using BarberShop.Application.UseCases.Service.Create;
using BarberShop.Application.UseCases.Service.GetAll;
using BarberShop.Application.UseCases.Service.Update;
using BarberShop.Application.UseCases.User.ChangeStatus;
using BarberShop.Application.UseCases.User.Create;
using BarberShop.Application.UseCases.User.Delete;
using BarberShop.Application.UseCases.User.GetById;
using BarberShop.Application.UseCases.User.GetPaginated;
using BarberShop.Application.UseCases.User.Update;
using BarberShop.Application.UseCases.ScheduleBlock.Create;
using BarberShop.Application.UseCases.ScheduleBlock.Delete;
using BarberShop.Application.UseCases.ScheduleBlock.GetByBarber;
using BarberShop.Application.UseCases.WorkingHours.GetByBarber;
using BarberShop.Application.UseCases.WorkingHours.Upsert;
using BarberShop.Infra.Interfaces;
using BarberShop.Infra.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MoneyScope.Infra.Repositories;
using Microsoft.Extensions.Configuration;
using BarberShop.Application.UseCases.Auth.ResetPasswwordById;

namespace BarberShop.IOC
{
    public static class Injection
    {
        public static IServiceCollection InjectDependencies(this IServiceCollection services, MigrationConfig? migrationConfig, IConfiguration? configuration = null)
        {
            // Repositories
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped(typeof(IBaseRelationRepository<>), typeof(BaseRelationRepository<>));
            services.AddScoped<IRepositoryFactory, RepositoryFactory>();

            // Support services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IBlobService, BlobService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddHttpClient<IMercadoPagoService, MercadoPagoService>();

            // Auth use cases
            services.AddScoped<ILoginUseCase, LoginUseCase>();
            services.AddScoped<IRegisterUseCase, RegisterUseCase>();
            services.AddScoped<ISocialLoginUseCase, SocialLoginUseCase>();
            services.AddScoped<ISendEmailResetPasswordUseCase, SendEmailResetPasswordUseCase>();
            services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
            services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
            services.AddScoped<IResetPasswordByIdUseCase, ResetPasswordByIdUseCase>();

            // User use cases
            services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
            services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();
            services.AddScoped<IChangeUserStatusUseCase, ChangeUserStatusUseCase>();
            services.AddScoped<IGetUserByIdUseCase, GetUserByIdUseCase>();
            services.AddScoped<IGetUsersPaginatedUseCase, GetUsersPaginatedUseCase>();
            services.AddScoped<IDeleteUserUseCase, DeleteUserUseCase>();

            // Appointment use cases
            services.AddScoped<ICreateAppointmentUseCase, CreateAppointmentUseCase>();
            services.AddScoped<IGetAvailableSlotsUseCase, GetAvailableSlotsUseCase>();
            services.AddScoped<ICancelAppointmentUseCase, CancelAppointmentUseCase>();
            services.AddScoped<IGetUserAppointmentsUseCase, GetUserAppointmentsUseCase>();
            services.AddScoped<IGetAppointmentByIdUseCase, GetAppointmentByIdUseCase>();

            // Payment use cases
            services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
            services.AddScoped<IProcessWebhookUseCase, ProcessWebhookUseCase>();
            services.AddScoped<IGetPaymentHistoryUseCase, GetPaymentHistoryUseCase>();

            // Service use cases
            services.AddScoped<IGetServicesUseCase, GetServicesUseCase>();
            services.AddScoped<ICreateServiceUseCase, CreateServiceUseCase>();
            services.AddScoped<IUpdateServiceUseCase, UpdateServiceUseCase>();
            services.AddScoped<IChangeServiceStatusUseCase, ChangeServiceStatusUseCase>();

            // Barber use cases
            services.AddScoped<IGetBarbersUseCase, GetBarbersUseCase>();
            services.AddScoped<ICreateBarberUseCase, CreateBarberUseCase>();
            services.AddScoped<IUpdateBarberUseCase, UpdateBarberUseCase>();
            services.AddScoped<IChangeBarberStatusUseCase, ChangeBarberStatusUseCase>();

            // WorkingHours use cases
            services.AddScoped<IGetWorkingHoursByBarberUseCase, GetWorkingHoursByBarberUseCase>();
            services.AddScoped<IUpsertWorkingHoursUseCase, UpsertWorkingHoursUseCase>();

            // ScheduleBlock use cases
            services.AddScoped<IGetScheduleBlocksByBarberUseCase, GetScheduleBlocksByBarberUseCase>();
            services.AddScoped<ICreateScheduleBlockUseCase, CreateScheduleBlockUseCase>();
            services.AddScoped<IDeleteScheduleBlockUseCase, DeleteScheduleBlockUseCase>();

            return services;
        }
    }
}
