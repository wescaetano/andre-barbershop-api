using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.AccessControl
{
    public class ProfileModule : BaseEntityRelation
    {
        public long ModuleId { get; set; }
        public long ProfileId { get; set; }


        // Permissions
        public bool Visualize { get; set; }
        public bool Edit { get; set; }
        public bool Register { get; set; }
        public bool Inactivate { get; set; }
        public bool Exclude { get; set; }


        // Navigation properties
        public virtual Module Module { get; set; } = null!;
        public virtual Profile Profile { get; set; } = null!;
    }
}
