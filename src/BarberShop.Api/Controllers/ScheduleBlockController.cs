using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.ScheduleBlock.Create;
using BarberShop.Application.UseCases.ScheduleBlock.Delete;
using BarberShop.Application.UseCases.ScheduleBlock.GetByBarber;
using BarberShop.Communication.Models.ScheduleBlock;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Bloqueios de agenda por barbeiro</summary>
    [APIAuthorization("Barber-V", "Barber-E", "Barber-C", "Users-V", "Users-E")]
    public class ScheduleBlockController : BaseController
    {
        private readonly IGetScheduleBlocksByBarberUseCase _get;
        private readonly ICreateScheduleBlockUseCase _create;
        private readonly IDeleteScheduleBlockUseCase _delete;

        /// <summary></summary>
        public ScheduleBlockController(
            IGetScheduleBlocksByBarberUseCase get,
            ICreateScheduleBlockUseCase create,
            IDeleteScheduleBlockUseCase delete)
        {
            _get = get;
            _create = create;
            _delete = delete;
        }

        /// <summary>Lista bloqueios de um barbeiro em um intervalo</summary>
        [HttpGet("{barberId:long}")]
        public async Task<IActionResult> GetByBarber(
            [FromRoute] long barberId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
            => Result(await _get.ExecuteAsync(barberId, from, to));

        /// <summary>Cria um bloqueio de horário</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleBlockModel model)
            => Result(await _create.ExecuteAsync(model));

        /// <summary>Remove um bloqueio</summary>
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
            => Result(await _delete.ExecuteAsync(id));
    }
}
