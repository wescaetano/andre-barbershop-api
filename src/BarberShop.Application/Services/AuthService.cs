using BarberShop.Application.Interfaces;
using BarberShop.Communication.Enums.User;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Models.Report;
using BarberShop.Communication.Models.Token;
using BarberShop.Communication.Utils;
using BarberShop.Core.Enums.SendEmail;
using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MoneyScope.Application.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly SmtpConfig _configSmtp;
        private IHostingEnvironment _environment;
        public AuthService(IRepositoryFactory repositoryFactory, ITokenService tokenService, IOptions<SmtpConfig> configSmtp, IHostingEnvironment environment) : base(repositoryFactory)
        {
            _tokenService = tokenService;
            _configSmtp = configSmtp.Value;
            _environment = environment;
        }

        public async Task<ResponseModel<dynamic>> SocialLogin(SocialLoginModel model)
        {
            var userExist = await _repository<User>().Get(x => x.Email.ToLower() == model.Email.ToLower());

            if (userExist != null) // Verifica se o usuário já existe
            {
                var loginModel = new SocialLoginInternModel();
                loginModel.Login = model.Email;
                loginModel.ProviderId = model.ProviderId;

                if (string.IsNullOrEmpty(userExist.ProviderId) || !HashHelper.PasswordCompare(userExist.ProviderId, model.ProviderId))
                {

                    userExist.ProviderId = HashHelper.HashGeneration(model.ProviderId);
                    await _repository<User>().Update(userExist);
                }

                try
                {
                    return await AuthenticateSocialUser(loginModel);
                }
                catch (Exception ex)
                {
                    return FactoryResponse<dynamic>.BadRequestErroInterno(ex.Message);
                }
            }
            else // Se o usuário não existir, cria um novo usuário
            {
                var user = new User()
                {
                    Name = model.Name,
                    Email = model.Email,
                    Status = EUserStatus.Ativo,
                    Password = HashHelper.HashGeneration(model.ProviderId),
                    ProviderId = HashHelper.HashGeneration(model.ProviderId),
                    ProfilesUsers = new List<ProfileUser> { new ProfileUser { ProfileId = 2 } }
                };
                user.AddCreationDate();

                try
                {
                    await _repository<User>().Create(user);

                    // Cria o login model para autenticar o novo usuário
                    var loginModel = new SocialLoginInternModel();
                    loginModel.Login = user.Email;
                    loginModel.ProviderId = model.ProviderId;

                    return await AuthenticateSocialUser(loginModel);
                }
                catch (Exception ex)
                {
                    return FactoryResponse<dynamic>.BadRequestErroInterno(ex.Message);
                }
            }
        }
        private async Task<ResponseModel<dynamic>> AuthenticateSocialUser(SocialLoginInternModel model)
        {
            var user = _repository<User>().GetAllWithInclude(x => x.Email != null && x.Email == model.Login, setIncludes: query => query.Include(u => u.ProfilesUsers).ThenInclude(up => up.Profile)).FirstOrDefault();

            if (user == null) return FactoryResponse<dynamic>.BadRequest("Email Incorreto");

            if (user.Status == EUserStatus.Inativo) return FactoryResponse<dynamic>.BadRequest("Acesso bloqueado por inativação da conta.");

            if (!HashHelper.PasswordCompare(user.ProviderId!, model.ProviderId)) return FactoryResponse<dynamic>.BadRequest("Senha Incorreta");

            ResponseModel<dynamic> retorno = await _tokenService.GetToken(user);

            return retorno;
        }
        public async Task<ResponseModel<dynamic>> AuthenticateUser(LoginModel model)
        {
            var user = await _repository<User>().GetWithInclude(x => x.Email != null && x.Email == model.Login, setIncludes: query => query.Include(u => u.ProfilesUsers).ThenInclude(up => up.Profile));

            if (user == null || string.IsNullOrEmpty(user.Password))
                throw new Exception("Usuário não encontrado ou sem senha definida.");

            if (string.IsNullOrEmpty(model.Password))
                throw new Exception("Senha não informada.");

            if (!HashHelper.PasswordCompare(user.Password, model.Password))
                throw new Exception("Senha incorreta.");

            if (user.Status == EUserStatus.Inativo) return FactoryResponse<dynamic>.BadRequest("Acesso bloqueado por inativação da conta.");

            if (!HashHelper.PasswordCompare(user.Password!, model.Password)) return FactoryResponse<dynamic>.BadRequest("Senha Incorreta!");

            ResponseModel<dynamic> retorno = await  _tokenService.GetToken(user);

            return retorno;
        }

        // Methods related to password reset and email sending
        private string GetBodyResetPassword(string token, ERedefinitionEmailType type = ERedefinitionEmailType.RequestToResetPassword, string nome = "")
        {
            var path = Path.Combine(_environment.WebRootPath, $"MailTemplates/{"RedefinirSenha"}.html");
            var body = File.ReadAllText(path);

            string titulo = "";
            string texto1 = "";
            string texto2 = "";

            switch (type)
            {
                case ERedefinitionEmailType.RequestToResetPassword:
                    titulo = "Solicitação de Redefinição de Senha";
                    texto1 = $"Olá, {nome}!<br><br> Recebemos uma solicitação para redefinir sua senha de acesso ao MoneyScope.<br>Para continuar, clique no botão abaixo:";
                    texto2 = "Se você não fez essa solicitação, apenas ignore este e-mail. Sua conta permanecerá segura.";
                    break;
                case ERedefinitionEmailType.Unsubscribe:
                    titulo = "Cancelamento de inscricão";
                    texto1 = $"Olá, {nome}!<br><br> Sua inscrição foi cancelada com sucesso.";
                    texto2 = "Se você não fez essa solicitação, apenas ignore este e-mail. Sua conta permanecerá segura.";
                    break;
                default:
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
        public async Task<ResponseModel<dynamic>> ResetPassword(string token, string newPassword)
        {
            var isvalid = _tokenService.IsValidToken(token);
            if (!isvalid) return FactoryResponse<dynamic>.Forbiden("Token invalido");

            var login = _tokenService.Getclaim("unique_name", token);
            if (login is null) return FactoryResponse<dynamic>.Forbiden("Token invalido");

            var user = await _repository<User>().Get(x => (x.Email.Replace(" ", "") == login.Replace(" ", "")));
            if (user is null) return FactoryResponse<dynamic>.NotFound("Usuário não encontrado!");

            user.Status = EUserStatus.Ativo;
            user.Password = HashHelper.HashGeneration(newPassword);
            //       if (!usuario.IsValid) return FactoryResponse<dynamic>.InvalidModel(null, usuario.ValidationErrors);

            try
            {
                await _repository<User>().Update(user);
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequest($"erro interno: " + e.Message);
            }

            //var retornoToken = await GetToken(usuario, null, 86400);

            return FactoryResponse<dynamic>.Success("Senha definida com sucesso!");
        }
        private string GetBodyGenericEmail(string titulo, string texto1, string texto2)
        {
            var path = Path.Combine(_environment.WebRootPath, $"MailTemplates/{"ConfirmarPagamento"}.html");
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

                    var attachment = new Attachment(
                        stream,
                        att.FileName,
                        att.ContentType
                    );

                    message.Attachments.Add(attachment);
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
        public async Task<bool> SendEmail(SendEmailModel model, ERedefinitionEmailType type, string name)
        {
            try
            {
                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(_configSmtp.EmailFrom, _configSmtp.Password),
                    EnableSsl = true, // Gmail exige STARTTLS
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
        public async Task<ResponseModel<dynamic>> SendEmailResetPassword(string email, ERedefinitionEmailType type = ERedefinitionEmailType.RequestToResetPassword)
        {
            var usuario = await _repository<User>().Get(x => (x.Email != null && (x.Email.ToLower().Replace(" ", "") == email.ToLower().Replace(" ", ""))));
            if (usuario is null) return FactoryResponse<dynamic>.NotFound("Usuario não encontrado!");

            var tokenEmail = await _tokenService.GenerateTokenByEmail(email);

            var sendEmail = await SendEmail(new SendEmailModel("Envio de senha", usuario.Email, tokenEmail.Data!), type, usuario.Name);
            if (!sendEmail) return FactoryResponse<dynamic>.BadRequest("Erro ao Enviar Email!");
            return FactoryResponse<dynamic>.Success("Email enviado com sucesso!");
        }
    }
}
