using BarberShop.Application.UseCases.Payment.GetHistory;
using BarberShop.Communication.Enums.Payment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Payment;

public class GetPaymentHistoryUseCaseTests
{
    private readonly IBaseRepository<Domain.Payment> _paymentRepo;
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly GetPaymentHistoryUseCase _sut;

    public GetPaymentHistoryUseCaseTests()
    {
        _paymentRepo = Substitute.For<IBaseRepository<Domain.Payment>>();
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _sut = new GetPaymentHistoryUseCase(_paymentRepo, _appointmentRepo);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidUserId_ReturnsValidationError()
    {
        var result = await _sut.ExecuteAsync(0);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_NoPayments_ReturnsEmptyList()
    {
        _appointmentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new List<Domain.Appointment>());
        _paymentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Payment, bool>>>())
            .Returns(new List<Domain.Payment>());

        var result = await _sut.ExecuteAsync(1);

        Assert.True(result.Success);
        var list = result.Data as List<object>;
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task ExecuteAsync_WithPayments_ReturnsHistory()
    {
        var appointments = new List<Domain.Appointment>
        {
            new Domain.Appointment { Id = 1, UserId = 1, StartTime = new DateTime(2026, 3, 15, 10, 0, 0) },
            new Domain.Appointment { Id = 2, UserId = 1, StartTime = new DateTime(2026, 3, 20, 14, 0, 0) }
        };
        var payments = new List<Domain.Payment>
        {
            new Domain.Payment { Id = 1, AppointmentId = 1, Amount = 30, Status = EPaymentStatus.Approved },
            new Domain.Payment { Id = 2, AppointmentId = 2, Amount = 30, Status = EPaymentStatus.Approved }
        };

        _appointmentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(appointments);
        _paymentRepo.GetAll(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Payment, bool>>>())
            .Returns(payments);

        var result = await _sut.ExecuteAsync(1);

        Assert.True(result.Success);
        var list = result.Data as List<object>;
        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
    }
}
