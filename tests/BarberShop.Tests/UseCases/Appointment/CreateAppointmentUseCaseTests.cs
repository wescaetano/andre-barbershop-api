using BarberShop.Application.UseCases.Appointment.Create;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Appointment;

public class CreateAppointmentUseCaseTests
{
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly IBaseRepository<Domain.Service> _serviceRepo;
    private readonly CreateAppointmentUseCase _sut;

    private static readonly Domain.Service _defaultService = new()
    {
        Id = 1,
        Name = "Corte",
        DurationMinutes = 30,
        Price = 35
    };

    public CreateAppointmentUseCaseTests()
    {
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _serviceRepo = Substitute.For<IBaseRepository<Domain.Service>>();
        _sut = new CreateAppointmentUseCase(_appointmentRepo, _serviceRepo);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ReturnsNotFound()
    {
        _serviceRepo.Get(Arg.Any<long>()).Returns((Domain.Service?)null);

        var model = new CreateAppointmentModel
        {
            UserId = 1,
            BarberId = 1,
            ServiceId = 99,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            StartTime = new TimeOnly(10, 0)
        };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_SlotOccupied_ReturnsConflict()
    {
        _serviceRepo.Get(Arg.Any<long>()).Returns(_defaultService);
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new Domain.Appointment { Status = EAppointmentStatus.WaitingPayment });

        var model = new CreateAppointmentModel
        {
            UserId = 1,
            BarberId = 1,
            ServiceId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            StartTime = new TimeOnly(10, 0)
        };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModel_CreatesAppointmentWithCorrectEndTime()
    {
        _serviceRepo.Get(Arg.Any<long>()).Returns(_defaultService);
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns((Domain.Appointment?)null);
        _appointmentRepo.Create(Arg.Any<Domain.Appointment>()).Returns(x => x.ArgAt<Domain.Appointment>(0));

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var model = new CreateAppointmentModel
        {
            UserId = 1,
            BarberId = 1,
            ServiceId = 1,
            Date = date,
            StartTime = new TimeOnly(10, 0)
        };

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        await _appointmentRepo.Received(1).Create(Arg.Is<Domain.Appointment>(a =>
            a.Status == EAppointmentStatus.WaitingPayment &&
            a.UserId == 1 &&
            a.BarberId == 1 &&
            a.ServiceId == 1 &&
            a.EndTime == a.StartTime.AddMinutes(_defaultService.DurationMinutes)));
    }
}
