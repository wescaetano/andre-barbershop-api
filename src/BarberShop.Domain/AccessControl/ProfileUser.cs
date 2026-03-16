using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.AccessControl
{
    public class ProfileUser : BaseEntityRelation
    {
        public long ProfileId { get; set; }
        public long UserId { get; set; }

        // Navigation properties
        public virtual Profile Profile { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
