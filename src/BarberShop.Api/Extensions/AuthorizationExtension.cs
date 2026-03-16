using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BarberShop.Api.Extensions
{
    /// <summary>
    ///
    /// </summary>
    public static class AuthorizationExtension
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        public static void AddAuthorizationConfiguration(this IServiceCollection services)
        {
            services.AddAuthorization(opt =>
            {
                opt.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build();

                List<string> modules =
                [
                    "Users",
                    "Auth",
                    "SendEmail"
                ];
                foreach (var m in modules)
                {
                    AddAccessByClaimsFuncionality(opt, $"{m}-C");
                    AddAccessByClaimsFuncionality(opt, $"{m}-E");
                    AddAccessByClaimsFuncionality(opt, $"{m}-V");
                    AddAccessByClaimsFuncionality(opt, $"{m}-I");
                    AddAccessByClaimsFuncionality(opt, $"{m}-EX");
                }
            });
        }

        private static void AddAccessByClaimsFuncionality(AuthorizationOptions options, string type)
        {
            options.AddPolicy($"{type}", a => AddAccessByClaims(a, [new Claim("modulo", type)]));
        }

        private static void AddAccessByClaims(AuthorizationPolicyBuilder builder, List<Claim> claims)
        {
            builder.RequireAssertion(c =>
            {
                try
                {
                    return ValidateMultiClaims(c.User, claims);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    return false;
                }
            });
        }

        private static bool ValidateMultiClaims(ClaimsPrincipal principal, List<Claim> claims)
        {
            bool allow = false;
            foreach (var claim in claims)
            {
                bool exist = principal.Claims.ToList().Exists(c => c.Value == claim.Value);
                if (exist) allow = true;
            }
            return allow;
        }
    }
}
