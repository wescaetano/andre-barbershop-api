using BarberShop.Application.Interfaces;
using BarberShop.Application.UseCases.Payment.Create;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models.Payment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Payment;

public class CreatePaymentUseCaseTests
{
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly IBaseRepository<Domain.Payment> _paymentRepo;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly CreatePaymentUseCase _sut;

    public CreatePaymentUseCaseTests()
    {
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _paymentRepo = Substitute.For<IBaseRepository<Domain.Payment>>();
        _mercadoPagoService = Substitute.For<IMercadoPagoService>();
        _sut = new CreatePaymentUseCase(_appointmentRepo, _paymentRepo, _mercadoPagoService);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidModel_ReturnsAllValidationErrors()
    {
        var model = new CreatePaymentModel { AppointmentId = 0, Amount = 0, ServiceTitle = "", NotificationUrl = "" };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("agendamento", result.Description?.ToLower());
        Assert.Contains("valor", result.Description?.ToLower());
        Assert.Contains("título", result.Description?.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_AppointmentNotFound_ReturnsNotFound()
    {
        var model = new CreatePaymentModel { AppointmentId = 99, Amount = 30, ServiceTitle = "Corte", NotificationUrl = "https://site.com/webhook" };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns((Domain.Appointment?)null);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_AppointmentNotWaitingPayment_ReturnsBadRequest()
    {
        var model = new CreatePaymentModel { AppointmentId = 1, Amount = 30, ServiceTitle = "Corte", NotificationUrl = "https://site.com/webhook" };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new Domain.Appointment { Id = 1, Status = EAppointmentStatus.Paid });

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModel_CreatesPaymentAndReturnsCheckoutUrl()
    {
        var model = new CreatePaymentModel { AppointmentId = 1, Amount = 30, ServiceTitle = "Corte", NotificationUrl = "https://site.com/webhook" };
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(new Domain.Appointment { Id = 1, Status = EAppointmentStatus.WaitingPayment });
        _mercadoPagoService.CreatePreferenceAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("https://mercadopago.com/checkout/xyz");
        _paymentRepo.Create(Arg.Any<Domain.Payment>()).Returns(x => x.ArgAt<Domain.Payment>(0));

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        await _paymentRepo.Received(1).Create(Arg.Any<Domain.Payment>());
        await _mercadoPagoService.Received(1).CreatePreferenceAsync("Corte", 30, "appointment_1", "https://site.com/webhook");
    }
}
