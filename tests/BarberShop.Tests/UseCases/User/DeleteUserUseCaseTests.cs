using BarberShop.Application.UseCases.User.Delete;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.User;

public class DeleteUserUseCaseTests
{
    private readonly IBaseRepository<Domain.User> _userRepo;
    private readonly DeleteUserUseCase _sut;

    public DeleteUserUseCaseTests()
    {
        _userRepo = Substitute.For<IBaseRepository<Domain.User>>();
        _sut = new DeleteUserUseCase(_userRepo);
    }

    [Fact]
    public async Task ExecuteAsync_IdZero_ReturnsBadRequest()
    {
        var result = await _sut.ExecuteAsync(0);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        await _userRepo.DidNotReceive().Get(Arg.Any<long>());
    }

    [Fact]
    public async Task ExecuteAsync_UserNotFound_ReturnsNotFound()
    {
        _userRepo.Get(1L).Returns((Domain.User?)null);

        var result = await _sut.ExecuteAsync(1);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyDeleted_ReturnsConflict()
    {
        var user = new Domain.User { Id = 1, Name = "João", Email = "joao@test.com" };
        user.AddExclusionDate();
        _userRepo.Get(1L).Returns(user);

        var result = await _sut.ExecuteAsync(1);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_SoftDeletesUser()
    {
        var user = new Domain.User { Id = 1, Name = "Maria", Email = "maria@test.com" };
        _userRepo.Get(1L).Returns(user);
        _userRepo.Update(Arg.Any<Domain.User>()).Returns(user);

        var result = await _sut.ExecuteAsync(1);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        await _userRepo.Received(1).Update(Arg.Is<Domain.User>(u => u.ExclusionDate.HasValue));
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_DoesNotHardDeleteUser()
    {
        var user = new Domain.User { Id = 1, Name = "Carlos", Email = "carlos@test.com" };
        _userRepo.Get(1L).Returns(user);
        _userRepo.Update(Arg.Any<Domain.User>()).Returns(user);

        await _sut.ExecuteAsync(1);

        await _userRepo.DidNotReceive().Remove(Arg.Any<Domain.User>());
    }
}
