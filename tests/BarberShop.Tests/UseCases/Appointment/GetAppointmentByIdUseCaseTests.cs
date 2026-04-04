using BarberShop.Application.UseCases.Appointment.GetById;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Appointment;

public class GetAppointmentByIdUseCaseTests
{
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly GetAppointmentByIdUseCase _sut;

    public GetAppointmentByIdUseCaseTests()
    {
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _sut = new GetAppointmentByIdUseCase(_appointmentRepo);
    }

    [Fact]
    public async Task ExecuteAsync_IdZero_ReturnsBadRequest()
    {
        var result = await _sut.ExecuteAsync(0);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ReturnsNotFound()
    {
        _appointmentRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>(),
            Arg.Any<Func<IQueryable<Domain.Appointment>, IQueryable<Domain.Appointment>>>())
            .Returns((Domain.Appointment?)null);

        var result = await _sut.ExecuteAsync(1);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_ReturnsAppointmentWithUserAndPayment()
    {
        var appointment = new Domain.Appointment
        {
            Id        = 1,
            UserId    = 10,
            StartTime = new DateTime(2024, 6, 1, 9, 0, 0),
            EndTime   = new DateTime(2024, 6, 1, 9, 30, 0),
            Status    = EAppointmentStatus.Paid,
            User      = new Domain.User { Id = 10, Name = "Ana", Email = "ana@test.com" },
            Payment   = new Domain.Payment
            {
                Id                = 5,
                Amount            = 50.00m,
                ExternalReference = "REF-001",
                Status            = Communication.Enums.Payment.EPaymentStatus.Approved
            }
        };

        _appointmentRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>(),
            Arg.Any<Func<IQueryable<Domain.Appointment>, IQueryable<Domain.Appointment>>>())
            .Returns(appointment);

        var result = await _sut.ExecuteAsync(1);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_AppointmentWithoutPayment_ReturnsNullPayment()
    {
        var appointment = new Domain.Appointment
        {
            Id        = 2,
            UserId    = 10,
            StartTime = new DateTime(2024, 6, 2, 10, 0, 0),
            EndTime   = new DateTime(2024, 6, 2, 10, 30, 0),
            Status    = EAppointmentStatus.WaitingPayment,
            User      = new Domain.User { Id = 10, Name = "Pedro", Email = "pedro@test.com" },
            Payment   = null
        };

        _appointmentRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>(),
            Arg.Any<Func<IQueryable<Domain.Appointment>, IQueryable<Domain.Appointment>>>())
            .Returns(appointment);

        var result = await _sut.ExecuteAsync(2);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
    }
}
