using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Models.Report;
using BarberShop.Communication.Models.Token;
using BarberShop.Core.Enums.SendEmail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseModel<dynamic>> AuthenticateUser(LoginModel model);
        Task<ResponseModel<dynamic>> SocialLogin(SocialLoginModel model);
        Task<ResponseModel<dynamic>> SendEmailResetPassword(string email, ERedefinitionEmailType type = ERedefinitionEmailType.RequestToResetPassword);
        Task<bool> SendEmail(SendEmailModel model, ERedefinitionEmailType type, string name);
        Task<bool> SendGenericEmail(string to, string subject, string titulo, string texto1, string texto2, List<EmailAttachment>? attachments = null);
        Task<ResponseModel<dynamic>> ResetPassword(string token, string newPassword);
    }
}
