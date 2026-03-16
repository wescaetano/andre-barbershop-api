using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.AccessControl
{
    public class Module : BaseEntity
    {
        public string Name { get; set; } = "";
        public bool Visualize { get; set; }
        public bool Edit { get; set; }
        public bool Register { get; set; }
        public bool Inactivate { get; set; }
        public bool Exclude { get; set; }
        public virtual ICollection<ProfileModule> ProfilesModules { get; private set; } = [];
    }
}
