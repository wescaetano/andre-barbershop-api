using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.GetAvailableSlots
{
    public class GetAvailableSlotsUseCase : IGetAvailableSlotsUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;
        private static readonly TimeOnly _openTime = new(9, 0);
        private static readonly TimeOnly _closeTime = new(18, 0);
        private const int SlotMinutes = 30;

        public GetAvailableSlotsUseCase(IBaseRepository<Domain.Appointment> appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
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

            var dayStart = model.Date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = model.Date.ToDateTime(TimeOnly.MaxValue);

            var occupied = await _appointmentRepository.GetAll(
                a => a.StartTime >= dayStart && a.StartTime <= dayEnd
                  && a.Status != EAppointmentStatus.Cancelled);

            var occupiedSlots = occupied.Select(a => TimeOnly.FromDateTime(a.StartTime)).ToHashSet();

            var allSlots = GenerateSlots();
            var availableSlots = allSlots
                .Where(s => !occupiedSlots.Contains(s))
                .Select(s => s.ToString("HH:mm"))
                .ToList();

            return FactoryResponse<dynamic>.Success(availableSlots);
        }

        private static List<TimeOnly> GenerateSlots()
        {
            var slots = new List<TimeOnly>();
            var current = _openTime;
            while (current < _closeTime)
            {
                slots.Add(current);
                current = current.AddMinutes(SlotMinutes);
            }
            return slots;
        }
    }
}
