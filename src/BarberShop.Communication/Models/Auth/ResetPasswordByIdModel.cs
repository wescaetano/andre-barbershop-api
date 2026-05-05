using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Auth
{
    public class ResetPasswordByIdModel
    {
        public long UserId { get; set; }
        public string NewPassword { get; set; } = null!;
    }
}
