using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.Barber.ChangeStatus;
using BarberShop.Application.UseCases.Barber.Create;
using BarberShop.Application.UseCases.Barber.GetAll;
using BarberShop.Application.UseCases.Barber.Update;
using BarberShop.Communication.Models.Barber;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Gerenciamento de barbeiros da barbearia</summary>
    public class BarberController : BaseController
    {
        private readonly IGetBarbersUseCase _getBarbers;
        private readonly ICreateBarberUseCase _create;
        private readonly IUpdateBarberUseCase _update;
        private readonly IChangeBarberStatusUseCase _changeStatus;

        /// <summary></summary>
        public BarberController(
            IGetBarbersUseCase getBarbers,
            ICreateBarberUseCase create,
            IUpdateBarberUseCase update,
            IChangeBarberStatusUseCase changeStatus)
        {
            _getBarbers = getBarbers;
            _create = create;
            _update = update;
            _changeStatus = changeStatus;
        }

        /// <summary>Lista barbeiros ativos (público)</summary>
        [AllowAnonymous]
        [HttpGet]
        //[APIAuthorization("Barber-V")]
        public async Task<IActionResult> GetActive()
            => Result(await _getBarbers.ExecuteAsync(activeOnly: true));

        /// <summary>Lista todos os barbeiros incluindo inativos (admin)</summary>
        [HttpGet("all")]
        //[APIAuthorization("Barber-V")]
        public async Task<IActionResult> GetAll()
            => Result(await _getBarbers.ExecuteAsync(activeOnly: false));

        /// <summary>Cria um novo barbeiro</summary>
        [HttpPost]
        //[APIAuthorization("Barber-C")]
        public async Task<IActionResult> Create([FromBody] CreateBarberModel model)
            => Result(await _create.ExecuteAsync(model));

        /// <summary>Atualiza um barbeiro</summary>
        [HttpPut]
        //[APIAuthorization("Barber-E")]
        public async Task<IActionResult> Update([FromBody] UpdateBarberModel model)
            => Result(await _update.ExecuteAsync(model));

        /// <summary>Ativa ou inativa um barbeiro</summary>
        [HttpPatch("status")]
        //[APIAuthorization("Barber-E")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeBarberStatusModel model)
            => Result(await _changeStatus.ExecuteAsync(model));
    }
}
