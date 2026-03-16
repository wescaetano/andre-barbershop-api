using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Auth
{
    public class ResetPasswordModel
    {
        [Required]
        public string NewPassword { get; set; } = null!;

        [Required]
        public string Token { get; set; } = null!;
    }
}
