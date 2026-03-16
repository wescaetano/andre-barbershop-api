using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Token
{
    public class GenerateTokenModel
    {
        public string UserEmail { get; set; } = null!;
        public string? UserName { get; set; }
        //public EAreaType? Area { get; set; }
        public string? UserProfileImage { get; set; }
        public long UserId { get; set; }
        public List<string> Modules { get; set; } = [];
        public ModulesModel? ModulesAssembled { get; set; }
        public int? Seconds { get; set; }
    }
}
