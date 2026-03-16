using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Auth
{
    public class SmtpConfig
    {
        public string UrlRedefinicao { get; set; } = string.Empty;
        public bool UseDefaultCredentials { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool EnableTls { get; set; }
        public string EmailFrom { get; set; } = string.Empty;
        public string NameFrom { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UrlLogo { get; set; } = string.Empty;
    }
}
