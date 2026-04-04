using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BarberShop.Infra.DataAccess
{
    public class BarberShopContextFactory : IDesignTimeDbContextFactory<BarberShopContext>
    {
        public BarberShopContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "BarberShop.Api"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("LocalConn");

            var optionsBuilder = new DbContextOptionsBuilder<BarberShopContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0)));

            return new BarberShopContext(optionsBuilder.Options);
        }
    }
}
