using BarberShop.Application.Interfaces;
using BarberShop.Application.UseCases.Payment.ProcessWebhook;
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Enums.Payment;
using BarberShop.Communication.Models.Payment;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Payment;

public class ProcessWebhookUseCaseTests
{
    private readonly IBaseRepository<Domain.Payment> _paymentRepo;
    private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly ProcessWebhookUseCase _sut;

    public ProcessWebhookUseCaseTests()
    {
        _paymentRepo = Substitute.For<IBaseRepository<Domain.Payment>>();
        _appointmentRepo = Substitute.For<IBaseRepository<Domain.Appointment>>();
        _mercadoPagoService = Substitute.For<IMercadoPagoService>();
        _sut = new ProcessWebhookUseCase(_paymentRepo, _appointmentRepo, _mercadoPagoService);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidModel_ReturnsValidationErrors()
    {
        var model = new ProcessWebhookModel { Type = "", Data = null! };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_NonPaymentType_ReturnsIgnored()
    {
        var model = new ProcessWebhookModel { Type = "subscription", Data = new ProcessWebhookModel.WebhookData { Id = "123" } };

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        Assert.Equal("Evento ignorado.", result.Data?.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_ApprovedPayment_UpdatesPaymentAndAppointmentToPaid()
    {
        var model = new ProcessWebhookModel { Type = "payment", Data = new ProcessWebhookModel.WebhookData { Id = "123456" } };
        var payment = new Domain.Payment { Id = 1, AppointmentId = 10, ExternalReference = "appointment_10", Status = EPaymentStatus.Pending };
        var appointment = new Domain.Appointment { Id = 10, Status = EAppointmentStatus.WaitingPayment };

        _mercadoPagoService.GetPaymentStatusAsync("123456")
            .Returns(("approved", "appointment_10", 30m));
        _paymentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Payment, bool>>>())
            .Returns(payment);
        _appointmentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Appointment, bool>>>())
            .Returns(appointment);
        _paymentRepo.Update(Arg.Any<Domain.Payment>()).Returns(payment);
        _appointmentRepo.Update(Arg.Any<Domain.Appointment>()).Returns(appointment);

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        await _paymentRepo.Received(1).Update(Arg.Is<Domain.Payment>(p => p.Status == EPaymentStatus.Approved));
        await _appointmentRepo.Received(1).Update(Arg.Is<Domain.Appointment>(a => a.Status == EAppointmentStatus.Paid));
    }

    [Fact]
    public async Task ExecuteAsync_RejectedPayment_UpdatesPaymentStatusOnly()
    {
        var model = new ProcessWebhookModel { Type = "payment", Data = new ProcessWebhookModel.WebhookData { Id = "123456" } };
        var payment = new Domain.Payment { Id = 1, AppointmentId = 10, ExternalReference = "appointment_10", Status = EPaymentStatus.Pending };

        _mercadoPagoService.GetPaymentStatusAsync("123456")
            .Returns(("rejected", "appointment_10", 30m));
        _paymentRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Payment, bool>>>())
            .Returns(payment);
        _paymentRepo.Update(Arg.Any<Domain.Payment>()).Returns(payment);

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        await _paymentRepo.Received(1).Update(Arg.Is<Domain.Payment>(p => p.Status == EPaymentStatus.Rejected));
        await _appointmentRepo.DidNotReceive().Update(Arg.Any<Domain.Appointment>());
    }
}
