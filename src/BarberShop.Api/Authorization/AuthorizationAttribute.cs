using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BarberShop.Api.Controllers;
using BarberShop.Communication.Models;

namespace BarberShop.Api.Authorization
{
    public class APIAuthorizationAttribute : TypeFilterAttribute
    {
        public APIAuthorizationAttribute(params string[] policies) : base(typeof(APIAuthorizationFilter))
        {
            Arguments = new object[] { policies };
        }
    }

    public class APIAuthorizationFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _config;

        public string[] Policies { get; set; }

        public APIAuthorizationFilter(IConfiguration config, params string[] policies)
        {
            _config = config;
            Policies = policies;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await next();
                return;
            }

            var token = context.HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = new JsonResult(FactoryResponse<dynamic>.Unauthorized("Token não informado."))
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            List<System.Security.Claims.Claim> claims;
            try
            {
                var key = Encoding.ASCII.GetBytes(_config["TokenConfigurations:Key"]!);
                var issuer = _config["TokenConfigurations:Issuer"];
                var audience = _config["TokenConfigurations:Audience"];
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validated);

                var jwt = (JwtSecurityToken)validated;
                claims = jwt.Claims.ToList();

                var userId = claims.FirstOrDefault(c => c.Type == "id")?.Value;
                var controller = context.Controller as BaseController;
                controller?.SetUsuarioId((long)Convert.ToDouble(userId));
            }
            catch
            {
                context.Result = new JsonResult(FactoryResponse<dynamic>.Unauthorized("Token inválido ou expirado."))
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // Check if any required policy value is present in the token claims
            foreach (var policy in Policies)
            {
                if (claims.Any(c => c.Value == policy))
                {
                    await next();
                    return;
                }
            }

            context.Result = new JsonResult(FactoryResponse<dynamic>.Forbiden("Usuário sem autorização de acesso."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
