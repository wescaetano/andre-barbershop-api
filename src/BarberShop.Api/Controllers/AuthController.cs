using BarberShop.Application.UseCases.Auth.Login;
using BarberShop.Application.UseCases.Auth.RefreshToken;
using BarberShop.Application.UseCases.Auth.Register;
using BarberShop.Application.UseCases.Auth.ResetPassword;
using BarberShop.Application.UseCases.Auth.ResetPasswwordById;
using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Application.UseCases.Auth.SocialLogin;
using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Models.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Autenticação e gerenciamento de sessão</summary>
    public class AuthController : BaseController
    {
        private readonly ILoginUseCase _loginUseCase;
        private readonly IRegisterUseCase _registerUseCase;
        private readonly ISocialLoginUseCase _socialLoginUseCase;
        private readonly ISendEmailResetPasswordUseCase _sendEmailResetPasswordUseCase;
        private readonly IResetPasswordUseCase _resetPasswordUseCase;
        private readonly IRefreshTokenUseCase _refreshTokenUseCase;
        private readonly IResetPasswordByIdUseCase _ResetPasswordByIdUseCase;

        /// <summary></summary>
        public AuthController(
            ILoginUseCase loginUseCase,
            IRegisterUseCase registerUseCase,
            ISocialLoginUseCase socialLoginUseCase,
            ISendEmailResetPasswordUseCase sendEmailResetPasswordUseCase,
            IResetPasswordUseCase resetPasswordUseCase,
            IRefreshTokenUseCase refreshTokenUseCase,
            IResetPasswordByIdUseCase resetPasswordByIdUseCase)
        {
            _loginUseCase = loginUseCase;
            _registerUseCase = registerUseCase;
            _socialLoginUseCase = socialLoginUseCase;
            _sendEmailResetPasswordUseCase = sendEmailResetPasswordUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
            _refreshTokenUseCase = refreshTokenUseCase;
            _ResetPasswordByIdUseCase = resetPasswordByIdUseCase;
        }

        /// <summary>Cria uma nova conta de cliente e envia e-mail para definir senha</summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await _registerUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Realiza login com e-mail e senha</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _loginUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Realiza login via provedor social (Google, Apple etc.)</summary>
        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin([FromBody] SocialLoginModel model)
        {
            var result = await _socialLoginUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Envia e-mail com link para redefinição de senha</summary>
        [HttpPost("send-reset-password")]
        public async Task<IActionResult> SendEmailResetPassword([FromQuery] string email)
        {
            var result = await _sendEmailResetPasswordUseCase.ExecuteAsync(email);
            return Result(result);
        }

        /// <summary>Redefine a senha usando o token enviado por e-mail</summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var result = await _resetPasswordUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Renova o access token usando um refresh token válido</summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromQuery] string token)
        {
            var result = await _refreshTokenUseCase.ExecuteAsync(token);
            return Result(result);
        }

        /// <summary>Renova o access token usando um refresh token válido</summary>
        [HttpPost("reset-password-id")]
        public async Task<IActionResult> ResetPasswordById([FromBody] ResetPasswordByIdModel model)
        {
            var result = await _ResetPasswordByIdUseCase.ExecuteAsync(model);
            return Result(result);
        }
    }
}
