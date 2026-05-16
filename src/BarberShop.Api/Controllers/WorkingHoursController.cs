using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.WorkingHours.GetByBarber;
using BarberShop.Application.UseCases.WorkingHours.Upsert;
using BarberShop.Communication.Models.WorkingHours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Horários de funcionamento por barbeiro</summary>
    [APIAuthorization("Barber-V", "Barber-E", "Users-V", "Users-E")]
    public class WorkingHoursController : BaseController
    {
        private readonly IGetWorkingHoursByBarberUseCase _get;
        private readonly IUpsertWorkingHoursUseCase _upsert;

        public WorkingHoursController(IGetWorkingHoursByBarberUseCase get, IUpsertWorkingHoursUseCase upsert)
        {
            _get = get;
            _upsert = upsert;
        }

        /// <summary>Retorna horários de funcionamento de um barbeiro (público)</summary>
        [AllowAnonymous]
        [HttpGet("{barberId:long}")]
        public async Task<IActionResult> GetByBarber([FromRoute] long barberId)
            => Result(await _get.ExecuteAsync(barberId));

        /// <summary>Salva horários de funcionamento (cria ou atualiza)</summary>
        [HttpPut]
        public async Task<IActionResult> Upsert([FromBody] UpsertWorkingHoursModel model)
            => Result(await _upsert.ExecuteAsync(model));
    }
}
