using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Token
{
    public class LoginModel
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
