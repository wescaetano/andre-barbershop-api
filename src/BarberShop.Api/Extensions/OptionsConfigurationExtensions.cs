

using BarberShop.Application.Config;

namespace BarberShop.Api.Extensions
{
    public static class OptionsConfigurationExtensions

    /// <summary>
    /// Métodos de extensão para configurar opções fortemente tipadas na coleção de serviços.
    /// </summary>
    /// <param name="services">A instância de <see cref="IServiceCollection"/> para adicionar a configuração.</param>
    /// <param name="configuration">A instância de <see cref="IConfiguration"/> contendo os dados de configuração.</param>
    /// <returns>A mesma instância de <see cref="IServiceCollection"/> para permitir encadeamento de chamadas.</returns>
    /// <remarks>
    /// Faz o binding da seção <see cref="MigrationConfig"/> da configuração para a respectiva classe de opções.
    /// </remarks>
    {

        public static IServiceCollection AddConfiguredOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MigrationConfig>(options =>
            {
                configuration.GetSection(nameof(MigrationConfig)).Bind(options);
            });

            return services;
        }
    }
}

