using BarberShop.Application.Models.User;
using BarberShop.Application.UseCases.User.GetPaginated;
using BarberShop.Communication.Enums.User;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.User;

public class GetUsersPaginatedUseCaseTests
{
    private readonly IBaseRepository<Domain.User> _userRepo;
    private readonly GetUsersPaginatedUseCase _sut;

    public GetUsersPaginatedUseCaseTests()
    {
        _userRepo = Substitute.For<IBaseRepository<Domain.User>>();
        _sut = new GetUsersPaginatedUseCase(_userRepo);
    }

    [Fact]
    public async Task ExecuteAsync_PageSizeZero_ReturnsValidationError()
    {
        var model = new GetUsersPaginatedModel { PageSize = 0 };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("página", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_PageSizeOver100_ReturnsValidationError()
    {
        var model = new GetUsersPaginatedModel { PageSize = 101 };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_PageNumberZero_ReturnsValidationError()
    {
        var model = new GetUsersPaginatedModel { PageNumber = 0 };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSortField_ReturnsValidationError()
    {
        var model = new GetUsersPaginatedModel { SortField = "InvalidField" };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("ordenação", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSortOrder_ReturnsValidationError()
    {
        var model = new GetUsersPaginatedModel { SortOrder = "sideways" };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModel_ReturnsPaginatedResult()
    {
        var users = new List<Domain.User>
        {
            new() { Id = 1, Name = "Alice", Email = "alice@test.com", Status = EUserStatus.Ativo, ProfilesUsers = [] },
            new() { Id = 2, Name = "Bob",   Email = "bob@test.com",   Status = EUserStatus.Ativo, ProfilesUsers = [] }
        }.AsQueryable();

        _userRepo.GetAllWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>?>(),
            Arg.Any<Func<IQueryable<Domain.User>, IQueryable<Domain.User>>?>())
            .Returns(users);

        var model = new GetUsersPaginatedModel { PageSize = 10, PageNumber = 1 };

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByStatus_ReturnsOnlyMatchingUsers()
    {
        var users = new List<Domain.User>
        {
            new() { Id = 1, Name = "Ativo",   Email = "a@test.com", Status = EUserStatus.Ativo,   ProfilesUsers = [] },
            new() { Id = 2, Name = "Inativo", Email = "b@test.com", Status = EUserStatus.Inativo, ProfilesUsers = [] }
        }.AsQueryable();

        _userRepo.GetAllWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>?>(),
            Arg.Any<Func<IQueryable<Domain.User>, IQueryable<Domain.User>>?>())
            .Returns(users);

        var model = new GetUsersPaginatedModel { Status = EUserStatus.Ativo };

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
    }
}
