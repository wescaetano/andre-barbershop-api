using BarberShop.Application.Models.User;
using BarberShop.Application.UseCases.User.ChangeStatus;
using BarberShop.Communication.Enums.User;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.User;

public class ChangeUserStatusUseCaseTests
{
    private readonly IBaseRepository<Domain.User> _userRepo;
    private readonly ChangeUserStatusUseCase _sut;

    public ChangeUserStatusUseCaseTests()
    {
        _userRepo = Substitute.For<IBaseRepository<Domain.User>>();
        _sut = new ChangeUserStatusUseCase(_userRepo);
    }

    [Fact]
    public async Task ExecuteAsync_IdZero_ReturnsValidationError()
    {
        var model = new ChangeUserStatusModel { Id = 0, Status = EUserStatus.Inativo };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotFound_ReturnsNotFound()
    {
        var model = new ChangeUserStatusModel { Id = 99, Status = EUserStatus.Inativo };
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>())
            .Returns((Domain.User?)null);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModel_ChangesStatus()
    {
        var model = new ChangeUserStatusModel { Id = 1, Status = EUserStatus.Inativo };
        var user = new Domain.User { Id = 1, Status = EUserStatus.Ativo };
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>())
            .Returns(user);
        _userRepo.Update(Arg.Any<Domain.User>()).Returns(user);

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        await _userRepo.Received(1).Update(Arg.Is<Domain.User>(u => u.Status == EUserStatus.Inativo));
    }
}
