using Microsoft.AspNetCore.Mvc;
using BarberShop.Communication.Models;

namespace BarberShop.Api.Controllers
{
    /// <summary>
    ///
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Id preenchido ao bater nas rotas com o token.
        /// </summary>
        public long UserId { get; private set; }


        ///// <summary>
        ///// retorno padrão.
        ///// </summary>
        /// <param name="responseModel"></param>
        ///// <returns></returns>
        protected ObjectResult Result<T>(ResponseModel<T> responseModel) =>
        StatusCode(responseModel.StatusCode, responseModel);

        internal void SetUsuarioId(long usuarioId) => UserId = usuarioId;
    }
}
