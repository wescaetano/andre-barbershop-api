using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.Appointment.Cancel;
using BarberShop.Application.UseCases.Appointment.Create;
using BarberShop.Application.UseCases.Appointment.GetAvailableSlots;
using BarberShop.Application.UseCases.Appointment.GetByBarber;
using BarberShop.Application.UseCases.Appointment.GetById;
using BarberShop.Application.UseCases.Appointment.GetByUser;
using BarberShop.Communication.Models.Appointment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Gerenciamento de agendamentos</summary>
    public class AppointmentController : BaseController
    {
        private readonly ICreateAppointmentUseCase _createAppointmentUseCase;
        private readonly IGetAvailableSlotsUseCase _getAvailableSlotsUseCase;
        private readonly ICancelAppointmentUseCase _cancelAppointmentUseCase;
        private readonly IGetUserAppointmentsUseCase _getUserAppointmentsUseCase;
        private readonly IGetAppointmentByIdUseCase _getAppointmentByIdUseCase;
        private readonly IGetBarberAppointmentsUseCase _getBarberAppointmentsUseCase;

        /// <summary></summary>
        public AppointmentController(
            ICreateAppointmentUseCase createAppointmentUseCase,
            IGetAvailableSlotsUseCase getAvailableSlotsUseCase,
            ICancelAppointmentUseCase cancelAppointmentUseCase,
            IGetUserAppointmentsUseCase getUserAppointmentsUseCase,
            IGetAppointmentByIdUseCase getAppointmentByIdUseCase,
            IGetBarberAppointmentsUseCase getBarberAppointmentsUseCase)
        {
            _createAppointmentUseCase = createAppointmentUseCase;
            _getAvailableSlotsUseCase = getAvailableSlotsUseCase;
            _cancelAppointmentUseCase = cancelAppointmentUseCase;
            _getUserAppointmentsUseCase = getUserAppointmentsUseCase;
            _getAppointmentByIdUseCase = getAppointmentByIdUseCase;
            _getBarberAppointmentsUseCase = getBarberAppointmentsUseCase;
        }

        /// <summary>Cria um novo agendamento para o usuário</summary>
        [HttpPost]
        //[APIAuthorization("Appointments-C")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentModel model)
        {
            var result = await _createAppointmentUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Retorna os horários disponíveis em uma data para um serviço</summary>
        [AllowAnonymous]
        [HttpGet("available-slots")]
        //[APIAuthorization("Appointments-V")]
        public async Task<IActionResult> GetAvailableSlots(
            [FromQuery] DateOnly date,
            [FromQuery] long barberId,
            [FromQuery] long serviceId)
        {
            var result = await _getAvailableSlotsUseCase.ExecuteAsync(
                new GetAvailableSlotsModel { Date = date, BarberId = barberId, ServiceId = serviceId });
            return Result(result);
        }

        /// <summary>Cancela um agendamento. Apenas o dono pode cancelar e somente no status WaitingPayment</summary>
        [HttpPatch("cancel")]
        //[APIAuthorization("Appointments-E")]
        public async Task<IActionResult> Cancel([FromBody] CancelAppointmentModel model)
        {
            var result = await _cancelAppointmentUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Lista todos os agendamentos de um usuário, do mais recente ao mais antigo, incluindo pagamento</summary>
        [HttpGet("user/{userId:long}")]
        //[APIAuthorization("Appointments-C")]
        public async Task<IActionResult> GetByUser([FromRoute] long userId)
        {
            var result = await _getUserAppointmentsUseCase.ExecuteAsync(userId);
            return Result(result);
        }

        /// <summary>Busca um agendamento pelo ID, incluindo usuário e dados de pagamento</summary>
        [HttpGet("{id:long}")]
        //[APIAuthorization("Appointments-C")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            var result = await _getAppointmentByIdUseCase.ExecuteAsync(id);
            return Result(result);
        }

        /// <summary>Lista agendamentos de um barbeiro em um intervalo de datas</summary>
        [HttpGet("barber/{barberId:long}")]
        //[APIAuthorization("Appointments-C")]
        public async Task<IActionResult> GetByBarber(
            [FromRoute] long barberId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var result = await _getBarberAppointmentsUseCase.ExecuteAsync(barberId, from, to);
            return Result(result);
        }
    }
}
