using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Token
{
    public class TokenConfigurations
    {
        public string Key { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public int RefreshTokenValidityMins { get; set; }
        public int TokenValidityMins { get; set; }
    }
}
