using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.Service.ChangeStatus;
using BarberShop.Application.UseCases.Service.Create;
using BarberShop.Application.UseCases.Service.GetAll;
using BarberShop.Application.UseCases.Service.Update;
using BarberShop.Communication.Models.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Gerenciamento de serviços da barbearia</summary>
    [APIAuthorization("Users-C", "Users-E", "Users-V", "Users-I")]
    public class ServiceController : BaseController
    {
        private readonly IGetServicesUseCase _getServices;
        private readonly ICreateServiceUseCase _create;
        private readonly IUpdateServiceUseCase _update;
        private readonly IChangeServiceStatusUseCase _changeStatus;

        /// <summary></summary>
        public ServiceController(
            IGetServicesUseCase getServices,
            ICreateServiceUseCase create,
            IUpdateServiceUseCase update,
            IChangeServiceStatusUseCase changeStatus)
        {
            _getServices = getServices;
            _create = create;
            _update = update;
            _changeStatus = changeStatus;
        }

        /// <summary>Lista serviços ativos (público)</summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetActive()
            => Result(await _getServices.ExecuteAsync(activeOnly: true));

        /// <summary>Lista todos os serviços incluindo inativos (admin)</summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
            => Result(await _getServices.ExecuteAsync(activeOnly: false));

        /// <summary>Cria um novo serviço</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceModel model)
            => Result(await _create.ExecuteAsync(model));

        /// <summary>Atualiza um serviço</summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateServiceModel model)
            => Result(await _update.ExecuteAsync(model));

        /// <summary>Ativa ou inativa um serviço</summary>
        [HttpPatch("status")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeServiceStatusModel model)
            => Result(await _changeStatus.ExecuteAsync(model));
    }
}
