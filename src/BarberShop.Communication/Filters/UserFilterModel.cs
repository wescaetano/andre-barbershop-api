using BarberShop.Communication.Enums.User;
using BarberShop.Communication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Filters
{
    public class UserFilterModel : FilterModel
    {
        public EUserStatus? Status { get; set; }
        public string? Email { get; set; }
    }
}
