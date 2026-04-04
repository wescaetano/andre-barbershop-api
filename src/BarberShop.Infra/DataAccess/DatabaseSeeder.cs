using BarberShop.Communication.Enums.Profile;
using BarberShop.Communication.Enums.User;
using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Infra.DataAccess
{
    public class DatabaseSeeder
    {
        // Default password: Admin@123
        private const string AdminPasswordHash = "$2a$12$4sHQj3OUBgi.syi7oQwAf.TCbjhMzvqedwYnoVKPx5PVHR19siKKK";

        public static void Seed(ModelBuilder modelBuilder)
        {
            SeedModules(modelBuilder);
            SeedProfiles(modelBuilder);
            SeedProfileModules(modelBuilder);
            SeedAdminUser(modelBuilder);
        }

        private static void SeedModules(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Module>().HasData(
                new Module { Id = 1, Name = "Users",        Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
                new Module { Id = 2, Name = "Auth",         Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
                new Module { Id = 3, Name = "SendEmail",    Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
                new Module { Id = 4, Name = "Appointments", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
                new Module { Id = 5, Name = "Payments",     Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            );
        }

        private static void SeedProfiles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profile>().HasData(
                new Profile { Id = 1, Name = "Admin",   Status = EProfileStatus.Ativo },
                new Profile { Id = 2, Name = "Cliente", Status = EProfileStatus.Ativo }
            );
        }

        private static void SeedProfileModules(ModelBuilder modelBuilder)
        {
            // Admin — acesso total a todos os módulos
            modelBuilder.Entity<ProfileModule>().HasData(
                new ProfileModule { ProfileId = 1, ModuleId = 1, Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
                new ProfileModule { ProfileId = 1, ModuleId = 2, Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
                new ProfileModule { ProfileId = 1, ModuleId = 3, Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
                new ProfileModule { ProfileId = 1, ModuleId = 4, Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
                new ProfileModule { ProfileId = 1, ModuleId = 5, Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            );

            // Cliente — pode gerenciar próprios agendamentos e pagamentos, sem acesso a gestão de usuários
            modelBuilder.Entity<ProfileModule>().HasData(
                new ProfileModule { ProfileId = 2, ModuleId = 2, Visualize = true, Edit = false, Register = true,  Inactivate = false, Exclude = false },
                new ProfileModule { ProfileId = 2, ModuleId = 4, Visualize = true, Edit = false, Register = true,  Inactivate = true,  Exclude = false },
                new ProfileModule { ProfileId = 2, ModuleId = 5, Visualize = true, Edit = false, Register = true,  Inactivate = false, Exclude = false }
            );
        }

        private static void SeedAdminUser(ModelBuilder modelBuilder)
        {
            var adminUser = new User
            {
                Name         = "Admin",
                Email        = "admin@barbershop.com",
                Password     = AdminPasswordHash,
                Status       = EUserStatus.Ativo,
                CreationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            adminUser.SetId(1);

            modelBuilder.Entity<User>().HasData(adminUser);

            modelBuilder.Entity<ProfileUser>().HasData(
                new ProfileUser { UserId = 1, ProfileId = 1 }
            );
        }
    }
}
