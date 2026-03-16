using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infra.DataAccess
{
    public class BarberShopContext(DbContextOptions<BarberShopContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken>  RefreshTokens { get; set; } = null!;
        public DbSet<ProfileUser>  ProfileUsers { get; set; } = null!;
        public DbSet<ProfileModule>  ProfileModules { get; set; } = null!;
        public DbSet<Profile>  Profiles { get; set; } = null!;
        public DbSet<Module>  Modules { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BarberShopContext).Assembly);
            //DateTimeConfig.ConfigDateTime(modelBuilder);
            EntitiesConfigurator.Configure(modelBuilder);
            DatabaseSeeder.Seed(modelBuilder);

        }
    }
}
