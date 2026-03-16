using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infra.DataAccess
{
    public class EntitiesConfigurator
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            //modelBuilder.ApplyConfiguration(new UserMap());
            //modelBuilder.ApplyConfiguration(new ProfileModuleMap());

        }
    }
}
