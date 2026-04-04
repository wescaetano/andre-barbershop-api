using BarberShop.Application.UseCases.User.GetById;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.User;

public class GetUserByIdUseCaseTests
{
    private readonly IBaseRepository<Domain.User> _userRepo;
    private readonly GetUserByIdUseCase _sut;

    public GetUserByIdUseCaseTests()
    {
        _userRepo = Substitute.For<IBaseRepository<Domain.User>>();
        _sut = new GetUserByIdUseCase(_userRepo);
    }

    [Fact]
    public async Task ExecuteAsync_IdZero_ReturnsBadRequest()
    {
        var result = await _sut.ExecuteAsync(0);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotFound_ReturnsNotFound()
    {
        _userRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>(),
            Arg.Any<Func<IQueryable<Domain.User>, IQueryable<Domain.User>>>())
            .Returns((Domain.User?)null);

        var result = await _sut.ExecuteAsync(1);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_ReturnsUser()
    {
        var user = new Domain.User
        {
            Id = 1,
            Name = "João",
            Email = "joao@test.com",
            ProfilesUsers = []
        };

        _userRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>(),
            Arg.Any<Func<IQueryable<Domain.User>, IQueryable<Domain.User>>>())
            .Returns(user);

        var result = await _sut.ExecuteAsync(1);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_UserWithProfiles_ReturnsProfilesInData()
    {
        var profile = new Profile { Id = 10, Name = "Admin" };
        var user = new Domain.User
        {
            Id = 1,
            Name = "Maria",
            Email = "maria@test.com",
            ProfilesUsers =
            [
                new ProfileUser { ProfileId = 10, UserId = 1, Profile = profile }
            ]
        };

        _userRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>(),
            Arg.Any<Func<IQueryable<Domain.User>, IQueryable<Domain.User>>>())
            .Returns(user);

        var result = await _sut.ExecuteAsync(1);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
    }
}
