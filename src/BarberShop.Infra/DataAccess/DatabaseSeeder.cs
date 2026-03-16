using BarberShop.Communication.Enums.Profile;
using BarberShop.Communication.Enums.User;
using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infra.DataAccess
{
    public class DatabaseSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {

            ////Modulos
            //modelBuilder.Entity<Module>().HasData(
            //    new Module { Id = 1, Name = "Users", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            //    );

            //modelBuilder.Entity<Module>().HasData(
            // new Module { Id = 2, Name = "Auth", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            // );

            //modelBuilder.Entity<Module>().HasData(
            // new Module { Id = 3, Name = "Goals", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            // );

            //modelBuilder.Entity<Module>().HasData(
            // new Module { Id = 4, Name = "Reports", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            // );

            //modelBuilder.Entity<Module>().HasData(
            // new Module { Id = 5, Name = "SendEmail", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            // );

            //modelBuilder.Entity<Module>().HasData(
            // new Module { Id = 6, Name = "Transactions", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            // );

            //modelBuilder.Entity<Module>().HasData(
            // new Module { Id = 7, Name = "TransactionCategories", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true }
            // );

            //modelBuilder.Entity<Profile>().HasData(
            //    new Profile { Id = 1, Name = "Admin", Status = EProfileStatus.Ativo },
            //    new Profile { Id = 2, Name = "User", Status = EProfileStatus.Ativo }
            //);

            ////PerfilFuncionalidade Admin
            //modelBuilder.Entity<ProfileModule>().HasData(
            //    new ProfileModule { ProfileId = 1, ModuleId = 1, Visualize = true, Edit = true, Register = true, Exclude = true, Inactivate = true },
            //    new ProfileModule { ProfileId = 1, ModuleId = 2, Visualize = true, Edit = true, Register = true, Exclude = true, Inactivate = true },
            //    new ProfileModule { ProfileId = 1, ModuleId = 3, Visualize = true, Edit = true, Register = true, Exclude = true, Inactivate = true },
            //    new ProfileModule { ProfileId = 1, ModuleId = 4, Visualize = true, Edit = true, Register = true, Exclude = true, Inactivate = true },
            //    new ProfileModule { ProfileId = 1, ModuleId = 5, Visualize = true, Edit = true, Register = true, Exclude = true, Inactivate = true },
            //    new ProfileModule { ProfileId = 1, ModuleId = 6, Visualize = true, Edit = true, Register = true, Exclude = true, Inactivate = true },
            //    new ProfileModule { ProfileId = 1, ModuleId = 7, Visualize = true, Edit = true, Register = true, Exclude = true, Inactivate = true }
            //    );

            ////PerfilFuncionalidade User
            //modelBuilder.Entity<ProfileModule>().HasData(
            //    new ProfileModule { ProfileId = 2, ModuleId = 1, Visualize = true, Edit = true, Register = true, Exclude = false, Inactivate = true },
            //    new ProfileModule { ProfileId = 2, ModuleId = 2, Visualize = true, Edit = true, Register = true, Exclude = false, Inactivate = false },
            //    new ProfileModule { ProfileId = 2, ModuleId = 3, Visualize = true, Edit = true, Register = true, Exclude = false, Inactivate = true },
            //    new ProfileModule { ProfileId = 2, ModuleId = 4, Visualize = true, Edit = true, Register = true, Exclude = false, Inactivate = true },
            //    new ProfileModule { ProfileId = 2, ModuleId = 5, Visualize = true, Edit = true, Register = true, Exclude = false, Inactivate = true },
            //    new ProfileModule { ProfileId = 2, ModuleId = 6, Visualize = true, Edit = true, Register = true, Exclude = false, Inactivate = true },
            //    new ProfileModule { ProfileId = 2, ModuleId = 7, Visualize = true, Edit = true, Register = true, Exclude = false, Inactivate = true }
            //    );



            ////usuario

            //var adminUser = new User
            //{
            //    Name = "Admin",
            //    Email = "admin@admin.com",
            //    Status = EUserStatus.Ativo,
            //    Password = "$2a$15$sBUdx5/OPL0bVk3XkHWPnuxFo3xn3zCBwbfUNmWFjBMJ6B4f.mjzi",
            //    CreationDate = DateTime.UtcNow
            //};
            //adminUser.SetId(1);


            ////Usuario
            //modelBuilder.Entity<User>().HasData(
            //   adminUser
            //);

            ////ProfileUser
            //modelBuilder.Entity<ProfileUser>().HasData(
            //    new ProfileUser
            //    {
            //        ProfileId = 1,
            //        UserId = 1
            //    }
            //);
        }
    }
}
