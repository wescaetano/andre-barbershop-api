using BarberShop.Application.UseCases.Appointment.GetAvailableSlots;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Appointment;

public class GetAvailableSlotsUseCaseTests
{
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly GetAvailableSlotsUseCase _sut;

    public GetAvailableSlotsUseCaseTests()
    {
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _sut = new GetAvailableSlotsUseCase(_appointmentRepo);
    }

    [Fact]
    public async Task ExecuteAsync_PastDate_ReturnsValidationError()
    {
        var model = new GetAvailableSlotsModel { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_NoAppointments_ReturnsAllSlots()
    {
        var model = new GetAvailableSlotsModel { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) };
        _appointmentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new List<Domain.Appointment>());

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        var slots = result.Data as List<string>;
        Assert.NotNull(slots);
        Assert.Contains("09:00", slots);
        Assert.Contains("17:30", slots);
        Assert.Equal(18, slots.Count); // 09:00 to 17:30, 30-min intervals = 18 slots
    }

    [Fact]
    public async Task ExecuteAsync_OccupiedSlot_IsRemovedFromAvailableList()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var model = new GetAvailableSlotsModel { Date = date };
        var occupiedStartTime = date.ToDateTime(new TimeOnly(10, 0));
        _appointmentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new List<Domain.Appointment>
            {
                new Domain.Appointment { StartTime = occupiedStartTime, Status = EAppointmentStatus.WaitingPayment }
            });

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        var slots = result.Data as List<string>;
        Assert.NotNull(slots);
        Assert.DoesNotContain("10:00", slots);
        Assert.Contains("09:30", slots);
        Assert.Contains("10:30", slots);
    }
}
