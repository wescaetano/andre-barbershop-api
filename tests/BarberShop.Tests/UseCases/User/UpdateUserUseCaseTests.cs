using BarberShop.Application.Models.User;
using BarberShop.Application.UseCases.User.Update;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.User;

public class UpdateUserUseCaseTests
{
    private readonly IBaseRepository<Domain.User> _userRepo;
    private readonly UpdateUserUseCase _sut;

    public UpdateUserUseCaseTests()
    {
        _userRepo = Substitute.For<IBaseRepository<Domain.User>>();
        _sut = new UpdateUserUseCase(_userRepo);
    }

    [Fact]
    public async Task ExecuteAsync_IdZero_ReturnsValidationError()
    {
        var model = new UpdateUserModel { Id = 0, Email = "nao-e-email" };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("id", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotFound_ReturnsNotFound()
    {
        var model = new UpdateUserModel { Id = 1, Name = "Novo Nome" };
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>())
            .Returns((Domain.User?)null);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModel_UpdatesUser()
    {
        var model = new UpdateUserModel { Id = 1, Name = "Novo Nome" };
        var existingUser = new Domain.User { Id = 1, Name = "Antigo Nome", Email = "user@test.com" };
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>())
            .Returns(existingUser);
        _userRepo.Update(Arg.Any<Domain.User>()).Returns(existingUser);

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        await _userRepo.Received(1).Update(Arg.Is<Domain.User>(u => u.Name == "Novo Nome"));
    }
}
