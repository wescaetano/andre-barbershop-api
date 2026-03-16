using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using BarberShop.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyScope.Application.Services
{
    public class ServiceBackGround : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceBackGround(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Executou o agendamento dos bgServices");
            var tz = TimeZoneInfo.FindSystemTimeZoneById(
             OperatingSystem.IsWindows()
                 ? "E. South America Standard Time"
                 : "America/Sao_Paulo"
             );

            var options = new RecurringJobOptions
            {
                TimeZone = tz
            };

            using (var scope = _serviceProvider.CreateScope())
            {
                var _recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                
                //_recurringJobManager.AddOrUpdate<IReportPdfService>(
                //    "envia-relatorios-mensais",
                //    service => service.SendMonthlyReportsToAllUsersAsync(),
                //    "0 8 1 * *",
                //    options
                //);

               
                //_recurringJobManager.AddOrUpdate<ITokenService>(
                //    "exclui-refresh-tokens-expirados",
                //    service => service.ExcludeExpiredRefreshTokens(),
                //    Cron.Hourly(),
                //    options
                //);
            }
            return Task.CompletedTask;
        }
    }
}
