using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Report
{
    public class EmailAttachment
    {
        public string FileName { get; set; } = null!;
        public byte[] Content { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }
}
