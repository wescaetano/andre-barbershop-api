using BarberShop.Application.UseCases.Appointment.Cancel;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Appointment;

public class CancelAppointmentUseCaseTests
{
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly CancelAppointmentUseCase _sut;

    public CancelAppointmentUseCaseTests()
    {
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _sut = new CancelAppointmentUseCase(_appointmentRepo);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidIds_ReturnsAllValidationErrors()
    {
        var model = new CancelAppointmentModel { AppointmentId = 0, UserId = 0 };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("agendamento", result.Description?.ToLower());
        Assert.Contains("usuário", result.Description?.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_AppointmentNotFound_ReturnsNotFound()
    {
        var model = new CancelAppointmentModel { AppointmentId = 99, UserId = 1 };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns((Domain.Appointment?)null);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_DifferentUser_ReturnsForbidden()
    {
        var model = new CancelAppointmentModel { AppointmentId = 1, UserId = 2 };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new Domain.Appointment { Id = 1, UserId = 1, Status = EAppointmentStatus.WaitingPayment });

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyPaid_ReturnsBadRequest()
    {
        var model = new CancelAppointmentModel { AppointmentId = 1, UserId = 1 };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new Domain.Appointment { Id = 1, UserId = 1, Status = EAppointmentStatus.Paid });

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_CancelsAppointment()
    {
        var model = new CancelAppointmentModel { AppointmentId = 1, UserId = 1 };
        var appointment = new Domain.Appointment { Id = 1, UserId = 1, Status = EAppointmentStatus.WaitingPayment };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(appointment);
        _appointmentRepo.Update(Arg.Any<Domain.Appointment>()).Returns(appointment);

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        await _appointmentRepo.Received(1).Update(Arg.Is<Domain.Appointment>(a => a.Status == EAppointmentStatus.Cancelled));
    }
}
