using BarberShop.Application.UseCases.Appointment.GetByUser;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Appointment;

public class GetUserAppointmentsUseCaseTests
{
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly GetUserAppointmentsUseCase _sut;

    public GetUserAppointmentsUseCaseTests()
    {
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _sut = new GetUserAppointmentsUseCase(_appointmentRepo);
    }

    [Fact]
    public async Task ExecuteAsync_UserIdZero_ReturnsBadRequest()
    {
        var result = await _sut.ExecuteAsync(0);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_NegativeUserId_ReturnsBadRequest()
    {
        var result = await _sut.ExecuteAsync(-1);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidUserId_ReturnsSuccess()
    {
        var appointments = new List<Domain.Appointment>
        {
            new()
            {
                Id        = 1,
                UserId    = 5,
                StartTime = new DateTime(2024, 6, 1, 9, 0, 0),
                EndTime   = new DateTime(2024, 6, 1, 9, 30, 0),
                Status    = EAppointmentStatus.WaitingPayment
            }
        }.AsQueryable();

        _appointmentRepo.GetAllWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>?>(),
            Arg.Any<Func<IQueryable<Domain.Appointment>, IQueryable<Domain.Appointment>>?>())
            .Returns(appointments);

        var result = await _sut.ExecuteAsync(5);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_NoAppointments_ReturnsEmptyList()
    {
        _appointmentRepo.GetAllWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>?>(),
            Arg.Any<Func<IQueryable<Domain.Appointment>, IQueryable<Domain.Appointment>>?>())
            .Returns(Enumerable.Empty<Domain.Appointment>().AsQueryable());

        var result = await _sut.ExecuteAsync(99);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
    }
}
