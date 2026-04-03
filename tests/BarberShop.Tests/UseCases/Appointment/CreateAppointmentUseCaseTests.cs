using BarberShop.Application.UseCases.Appointment.Create;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Appointment;

public class CreateAppointmentUseCaseTests
{
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly CreateAppointmentUseCase _sut;

    public CreateAppointmentUseCaseTests()
    {
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _sut = new CreateAppointmentUseCase(_appointmentRepo);
    }

    [Fact]
    public async Task ExecuteAsync_PastDate_ReturnsValidationError()
    {
        var model = new CreateAppointmentModel
        {
            UserId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            StartTime = new TimeOnly(10, 0)
        };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("passado", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_OutsideWorkingHours_ReturnsValidationError()
    {
        var model = new CreateAppointmentModel
        {
            UserId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            StartTime = new TimeOnly(20, 0)
        };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("09:00", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_SlotOccupied_ReturnsConflict()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var model = new CreateAppointmentModel { UserId = 1, Date = date, StartTime = new TimeOnly(10, 0) };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new Domain.Appointment { Status = EAppointmentStatus.WaitingPayment });

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModel_CreatesAppointmentWithWaitingPaymentStatus()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var model = new CreateAppointmentModel { UserId = 1, Date = date, StartTime = new TimeOnly(10, 0) };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns((Domain.Appointment?)null);
        _appointmentRepo.Create(Arg.Any<Domain.Appointment>()).Returns(x => x.ArgAt<Domain.Appointment>(0));

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        await _appointmentRepo.Received(1).Create(Arg.Is<Domain.Appointment>(a =>
            a.Status == EAppointmentStatus.WaitingPayment &&
            a.UserId == 1));
    }
}
