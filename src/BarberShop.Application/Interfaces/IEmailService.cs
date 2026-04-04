using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Models.Report;
using BarberShop.Core.Enums.SendEmail;

namespace BarberShop.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmail(SendEmailModel model, ERedefinitionEmailType type, string name);
        Task<bool> SendGenericEmail(string to, string subject, string titulo, string texto1, string texto2, List<EmailAttachment>? attachments = null);
    }
}
