using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BarberShop.Api.Controllers;
using BarberShop.Communication.Models;

namespace BarberShop.Api.Authorization
{
    /// <summary>
    ///
    /// </summary>
    public class APIAuthorizationAttribute : TypeFilterAttribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="policies"></param>
        public APIAuthorizationAttribute(params string[] policies) : base(typeof(APIAuthorizationFilter))
        {
            Arguments = new object[] { policies };
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class APIAuthorizationFilter : IActionFilter
    {
        private readonly IAuthorizationService _authService;

        /// <summary>
        ///
        /// </summary>
        public string[] Policies { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="policies"></param>
        public APIAuthorizationFilter(IAuthorizationService authService, params string[] policies)
        {
            Policies = policies;
            _authService = authService;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filterContext"></param>
        public async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var usuarioId = (string?)filterContext.HttpContext.Items["id"];

            var controller = filterContext.Controller as BaseController;

            controller?.SetUsuarioId((long)Convert.ToDouble(usuarioId));

            foreach (var p in Policies)
            {
                try
                {
                    if ((await _authService.AuthorizeAsync(filterContext.HttpContext.User, p)).Succeeded)
                        return;
                }
                catch
                {
                    break;
                }
            }
            filterContext.Result = new JsonResult(FactoryResponse<dynamic>.Forbiden("Usuário sem autorização de acesso!")) { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        { }
    }
}
