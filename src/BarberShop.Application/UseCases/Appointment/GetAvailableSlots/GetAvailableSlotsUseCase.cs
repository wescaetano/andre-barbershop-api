using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.GetAvailableSlots
{
    public class GetAvailableSlotsUseCase : IGetAvailableSlotsUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
        private readonly IBaseRepository<Domain.Service> _serviceRepo;
        private readonly IBaseRepository<Domain.WorkingHours> _workingHoursRepo;
        private readonly IBaseRepository<Domain.ScheduleBlock> _blockRepo;

        public GetAvailableSlotsUseCase(
            IBaseRepository<Domain.Appointment> appointmentRepo,
            IBaseRepository<Domain.Service> serviceRepo,
            IBaseRepository<Domain.WorkingHours> workingHoursRepo,
            IBaseRepository<Domain.ScheduleBlock> blockRepo)
        {
            _appointmentRepo = appointmentRepo;
            _serviceRepo = serviceRepo;
            _workingHoursRepo = workingHoursRepo;
            _blockRepo = blockRepo;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(GetAvailableSlotsModel model)
        {
            var service = await _serviceRepo.Get(model.ServiceId);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");

            var dayOfWeek = (int)model.Date.DayOfWeek;
            var workingHours = await _workingHoursRepo.Get(
                wh => wh.BarberId == model.BarberId && wh.DayOfWeek == dayOfWeek);

            if (workingHours == null || !workingHours.IsOpen)
                return FactoryResponse<dynamic>.Success(new List<string>());

            var allSlots = GenerateSlots(workingHours.OpenTime, workingHours.CloseTime, service.DurationMinutes);

            var dayStart = model.Date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = model.Date.ToDateTime(TimeOnly.MaxValue);

            var appointments = await _appointmentRepo.GetAll(
                a => a.BarberId == model.BarberId
                  && a.StartTime >= dayStart
                  && a.StartTime < dayEnd
                  && a.Status != EAppointmentStatus.Cancelled);

            var blocks = await _blockRepo.GetAll(
                b => b.BarberId == model.BarberId
                  && b.EndTime > dayStart
                  && b.StartTime < dayEnd);

            var now = DateTime.Now;
            var isToday = model.Date == DateOnly.FromDateTime(now);

            var available = allSlots.Where(slot =>
            {
                var slotStart = model.Date.ToDateTime(slot);
                var slotEnd = slotStart.AddMinutes(service.DurationMinutes);

                if (isToday && slotStart <= now) return false;

                bool hasAppointment = appointments.Any(a =>
                    a.StartTime < slotEnd && a.EndTime > slotStart);

                bool hasBlock = blocks.Any(b =>
                    b.StartTime < slotEnd && b.EndTime > slotStart);

                return !hasAppointment && !hasBlock;
            })
            .Select(s => s.ToString("HH:mm"))
            .ToList();

            return FactoryResponse<dynamic>.Success(available);
        }

        private static List<TimeOnly> GenerateSlots(TimeOnly open, TimeOnly close, int durationMinutes)
        {
            var slots = new List<TimeOnly>();
            var current = open;
            while (current.AddMinutes(durationMinutes) <= close)
            {
                slots.Add(current);
                current = current.AddMinutes(durationMinutes);
            }
            return slots;
        }
    }
}
