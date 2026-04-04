using BarberShop.Application.Interfaces;
using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Models.Report;
using BarberShop.Core.Enums.SendEmail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BarberShop.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpConfig _configSmtp;
        private readonly IHostingEnvironment _environment;

        public EmailService(IOptions<SmtpConfig> configSmtp, IHostingEnvironment environment)
        {
            _configSmtp = configSmtp.Value;
            _environment = environment;
        }

        public async Task<bool> SendEmail(SendEmailModel model, ERedefinitionEmailType type, string name)
        {
            try
            {
                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(_configSmtp.EmailFrom, _configSmtp.Password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var body = GetBodyResetPassword(model.Content, type, name);

                using var message = new MailMessage
                {
                    From = new MailAddress(_configSmtp.EmailFrom, _configSmtp.NameFrom, Encoding.UTF8),
                    Subject = model.Subject,
                    Body = body,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                message.To.Add(model.To);

                await client.SendMailAsync(message);
                return true;
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP error: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
                return false;
            }
        }

        public async Task<bool> SendGenericEmail(string to, string subject, string titulo, string texto1, string texto2, List<EmailAttachment>? attachments = null)
        {
            var client = new SmtpClient
            {
                UseDefaultCredentials = _configSmtp.UseDefaultCredentials,
                Host = _configSmtp.Host,
                Port = _configSmtp.Port,
                EnableSsl = _configSmtp.EnableSsl,
                Credentials = new NetworkCredential(_configSmtp.EmailFrom, _configSmtp.Password)
            };

            if (client.EnableSsl && ServicePointManager.ServerCertificateValidationCallback == null)
                ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

            var body = GetBodyGenericEmail(titulo, texto1, texto2);

            MailMessage message = new()
            {
                From = new MailAddress(_configSmtp.EmailFrom, _configSmtp.NameFrom, Encoding.UTF8),
                IsBodyHtml = true,
                Body = body,
                Priority = MailPriority.Normal,
                Subject = subject,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            message.To.Add(to);

            if (attachments != null && attachments.Any())
            {
                foreach (var att in attachments)
                {
                    var stream = new MemoryStream(att.Content);
                    message.Attachments.Add(new Attachment(stream, att.FileName, att.ContentType));
                }
            }

            try
            {
                await Task.Run(() => client.Send(message));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private string GetBodyResetPassword(string token, ERedefinitionEmailType type, string nome)
        {
            var path = Path.Combine(_environment.WebRootPath, "MailTemplates/RedefinirSenha.html");
            var body = File.ReadAllText(path);

            string titulo, texto1, texto2;

            switch (type)
            {
                case ERedefinitionEmailType.RequestToResetPassword:
                    titulo = "Solicitação de Redefinição de Senha";
                    texto1 = $"Olá, {nome}!<br><br> Recebemos uma solicitação para redefinir sua senha.<br>Para continuar, clique no botão abaixo:";
                    texto2 = "Se você não fez essa solicitação, apenas ignore este e-mail. Sua conta permanecerá segura.";
                    break;
                case ERedefinitionEmailType.Unsubscribe:
                    titulo = "Cancelamento de inscrição";
                    texto1 = $"Olá, {nome}!<br><br> Sua inscrição foi cancelada com sucesso.";
                    texto2 = "Se você não fez essa solicitação, apenas ignore este e-mail. Sua conta permanecerá segura.";
                    break;
                default:
                    titulo = "";
                    texto1 = "";
                    texto2 = "";
                    break;
            }

            var variables = new Dictionary<string, string>
            {
                { "#URLLOGO", _configSmtp.UrlLogo },
                { "#TituloEmail", titulo },
                { "#TEXTO1", texto1 },
                { "#URLREDEFINIR", $"{_configSmtp.UrlRedefinicao}/{token}" },
                { "#TEXTO2", texto2 }
            };

            foreach (var item in variables)
                body = body.Replace(item.Key, item.Value);

            return body;
        }

        private string GetBodyGenericEmail(string titulo, string texto1, string texto2)
        {
            var path = Path.Combine(_environment.WebRootPath, "MailTemplates/ConfirmarPagamento.html");
            var body = File.ReadAllText(path);

            var variables = new Dictionary<string, string>
            {
                { "#URLLOGO", _configSmtp.UrlLogo },
                { "#TituloEmail", titulo },
                { "#TEXTO1", texto1 },
                { "#TEXTO2", texto2 }
            };

            foreach (var item in variables)
                body = body.Replace(item.Key, item.Value);

            return body;
        }
    }
}
