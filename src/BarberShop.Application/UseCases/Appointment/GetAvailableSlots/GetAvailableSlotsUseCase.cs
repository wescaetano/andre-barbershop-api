using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.GetAvailableSlots
{
    public class GetAvailableSlotsUseCase : IGetAvailableSlotsUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;
        private readonly IBaseRepository<Domain.Service> _serviceRepository;
        private static readonly TimeOnly _openTime = new(9, 0);
        private static readonly TimeOnly _closeTime = new(18, 0);

        public GetAvailableSlotsUseCase(
            IBaseRepository<Domain.Appointment> appointmentRepository,
            IBaseRepository<Domain.Service> serviceRepository)
        {
            _appointmentRepository = appointmentRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(GetAvailableSlotsModel model)
        {
            var validator = new GetAvailableSlotsValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var service = await _serviceRepository.Get(model.ServiceId);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");

            var dayStart = model.Date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = model.Date.ToDateTime(TimeOnly.MaxValue);

            var occupied = await _appointmentRepository.GetAll(
                a => a.StartTime >= dayStart && a.StartTime <= dayEnd
                  && a.Status != EAppointmentStatus.Cancelled);

            var occupiedSlots = occupied.Select(a => TimeOnly.FromDateTime(a.StartTime)).ToHashSet();

            var allSlots = GenerateSlots(service.DurationMinutes);
            var now = DateTime.Now;
            var isToday = model.Date == DateOnly.FromDateTime(now);

            var available = allSlots
                .Where(s =>
                {
                    if (isToday && model.Date.ToDateTime(s) <= now) return false;
                    return !occupiedSlots.Contains(s);
                })
                .Select(s => s.ToString("HH:mm"))
                .ToList();

            return FactoryResponse<dynamic>.Success(available);
        }

        private static List<TimeOnly> GenerateSlots(int durationMinutes)
        {
            var slots = new List<TimeOnly>();
            var current = _openTime;
            while (current.AddMinutes(durationMinutes) <= _closeTime)
            {
                slots.Add(current);
                current = current.AddMinutes(durationMinutes);
            }
            return slots;
        }
    }
}
