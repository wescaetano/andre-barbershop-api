using Hangfire.Dashboard.BasicAuthorization;

namespace BarberShop.Api.Extensions
{
    public class HangFireDashboard
    {
        public static BasicAuthAuthorizationFilter[] AuthAuthorizationFilters()
        {
            return [new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions{
        SslRedirect = false,

        RequireSsl = false,
        LoginCaseSensitive = true,

        Users = new []{
            new BasicAuthAuthorizationUser{
                Login = "admin",
                PasswordClear = "admin"
            }
        }
    }){
    }];
        }
    }
}
