using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Auth
{
    public class SendEmailModel
    {
        public string Subject { get; } = null!;
        public string To { get; } = null!;

        public string Content { get; } = null!;
        public string HtmlContent { get; } = string.Empty;

        public SendEmailModel(string subject, string to, string content, string htmlContent = "")
        {
            Subject = subject;
            To = to;
            Content = content;
            HtmlContent = htmlContent;
        }
    }
}
