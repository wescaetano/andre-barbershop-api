using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Token
{
    public class ModulesModel
    {
        public InfoProfileModel? InfoProfile { get; set; }
        public List<ModuleProfileUserModel>? ModuleProfileUser { get; set; }
        public class InfoProfileModel
        {
            public string? Name { get; set; }
            public long? IdProfile { get; set; }
        }
        public class ModuleProfileUserModel
        {
            public string? Name { get; set; }
            public long? IdModule { get; set; }
            public bool Visualize { get; set; } = false;
            public bool Edit { get; set; } = false;
            public bool Register { get; set; } = false;
            public bool Inactivate { get; set; } = false;
            public bool Exclude { get; set; } = false;
        }
    }
}
