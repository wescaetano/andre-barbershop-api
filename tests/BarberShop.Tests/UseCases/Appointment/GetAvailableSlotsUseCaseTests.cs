using BarberShop.Application.UseCases.Appointment.GetAvailableSlots;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Appointment;

public class GetAvailableSlotsUseCaseTests
{
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly IBaseRepository<Domain.Service> _serviceRepo;
    private readonly IBaseRepository<Domain.WorkingHours> _workingHoursRepo;
    private readonly IBaseRepository<Domain.ScheduleBlock> _blockRepo;
    private readonly GetAvailableSlotsUseCase _sut;

    private static readonly Domain.Service _defaultService = new()
    {
        Id = 1,
        Name = "Corte",
        DurationMinutes = 30,
        Price = 35
    };

    private static readonly Domain.WorkingHours _openAllDay = new()
    {
        BarberId = 1,
        DayOfWeek = 1, // Monday
        IsOpen = true,
        OpenTime = new TimeOnly(9, 0),
        CloseTime = new TimeOnly(18, 0)
    };

    public GetAvailableSlotsUseCaseTests()
    {
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _serviceRepo = Substitute.For<IBaseRepository<Domain.Service>>();
        _workingHoursRepo = Substitute.For<IBaseRepository<Domain.WorkingHours>>();
        _blockRepo = Substitute.For<IBaseRepository<Domain.ScheduleBlock>>();
        _sut = new GetAvailableSlotsUseCase(_appointmentRepo, _serviceRepo, _workingHoursRepo, _blockRepo);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ReturnsNotFound()
    {
        _serviceRepo.Get(Arg.Any<long>()).Returns((Domain.Service?)null);

        var model = new GetAvailableSlotsModel
        {
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            BarberId = 1,
            ServiceId = 99
        };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_BarberClosedThatDay_ReturnsEmptyList()
    {
        _serviceRepo.Get(Arg.Any<long>()).Returns(_defaultService);
        _workingHoursRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.WorkingHours, bool>>>())
            .Returns((Domain.WorkingHours?)null);

        var model = new GetAvailableSlotsModel
        {
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            BarberId = 1,
            ServiceId = 1
        };

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        var slots = result.Data as List<string>;
        Assert.NotNull(slots);
        Assert.Empty(slots);
    }

    [Fact]
    public async Task ExecuteAsync_NoAppointmentsOrBlocks_ReturnsAllSlotsInWorkingHours()
    {
        _serviceRepo.Get(Arg.Any<long>()).Returns(_defaultService);
        _workingHoursRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.WorkingHours, bool>>>())
            .Returns(_openAllDay);
        _appointmentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new List<Domain.Appointment>());
        _blockRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.ScheduleBlock, bool>>>())
            .Returns(new List<Domain.ScheduleBlock>());

        var model = new GetAvailableSlotsModel
        {
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            BarberId = 1,
            ServiceId = 1
        };

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
        _serviceRepo.Get(Arg.Any<long>()).Returns(_defaultService);
        _workingHoursRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.WorkingHours, bool>>>())
            .Returns(_openAllDay);

        var occupiedStart = date.ToDateTime(new TimeOnly(10, 0));
        _appointmentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new List<Domain.Appointment>
            {
                new() { StartTime = occupiedStart, EndTime = occupiedStart.AddMinutes(30), Status = EAppointmentStatus.WaitingPayment }
            });
        _blockRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.ScheduleBlock, bool>>>())
            .Returns(new List<Domain.ScheduleBlock>());

        var model = new GetAvailableSlotsModel { Date = date, BarberId = 1, ServiceId = 1 };

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        var slots = result.Data as List<string>;
        Assert.NotNull(slots);
        Assert.DoesNotContain("10:00", slots);
        Assert.Contains("09:30", slots);
        Assert.Contains("10:30", slots);
    }

    [Fact]
    public async Task ExecuteAsync_BlockedSlot_IsRemovedFromAvailableList()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        _serviceRepo.Get(Arg.Any<long>()).Returns(_defaultService);
        _workingHoursRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.WorkingHours, bool>>>())
            .Returns(_openAllDay);
        _appointmentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new List<Domain.Appointment>());

        var blockStart = date.ToDateTime(new TimeOnly(14, 0));
        _blockRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.ScheduleBlock, bool>>>())
            .Returns(new List<Domain.ScheduleBlock>
            {
                new() { BarberId = 1, StartTime = blockStart, EndTime = blockStart.AddMinutes(30) }
            });

        var model = new GetAvailableSlotsModel { Date = date, BarberId = 1, ServiceId = 1 };

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        var slots = result.Data as List<string>;
        Assert.NotNull(slots);
        Assert.DoesNotContain("14:00", slots);
        Assert.Contains("13:30", slots);
        Assert.Contains("14:30", slots);
    }
}
