using BarberShop.Application.Interfaces;
using BarberShop.Application.UseCases.Auth.Login;
using BarberShop.Communication.Enums.User;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Token;
using BarberShop.Communication.Utils;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Auth;

public class LoginUseCaseTests
{
    private readonly IBaseRepository<Domain.User> _userRepo;
    private readonly ITokenService _tokenService;
    private readonly LoginUseCase _sut;

    public LoginUseCaseTests()
    {
        _userRepo = Substitute.For<IBaseRepository<Domain.User>>();
        _tokenService = Substitute.For<ITokenService>();
        _sut = new LoginUseCase(_userRepo, _tokenService);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyFields_ReturnsAllValidationErrors()
    {
        var model = new LoginModel { Login = "", Password = "" };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("login", result.Description);
        Assert.Contains("senha", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidEmail_ReturnsValidationError()
    {
        var model = new LoginModel { Login = "nao-e-email", Password = "123456" };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("email", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotFound_ReturnsUnauthorized()
    {
        var model = new LoginModel { Login = "joao@test.com", Password = "123456" };
        _userRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>(),
            Arg.Any<Func<System.Linq.IQueryable<Domain.User>, System.Linq.IQueryable<Domain.User>>>())
            .Returns((Domain.User?)null);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WrongPassword_ReturnsUnauthorized()
    {
        var model = new LoginModel { Login = "joao@test.com", Password = "senha_errada" };
        var user = new Domain.User { Email = "joao@test.com", Password = HashHelper.HashGeneration("senha_correta"), Status = EUserStatus.Ativo };
        _userRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>(),
            Arg.Any<Func<System.Linq.IQueryable<Domain.User>, System.Linq.IQueryable<Domain.User>>>())
            .Returns(user);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_InactiveUser_ReturnsBadRequest()
    {
        var password = "senha123";
        var model = new LoginModel { Login = "joao@test.com", Password = password };
        var user = new Domain.User { Email = "joao@test.com", Password = HashHelper.HashGeneration(password), Status = EUserStatus.Inativo };
        _userRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>(),
            Arg.Any<Func<System.Linq.IQueryable<Domain.User>, System.Linq.IQueryable<Domain.User>>>())
            .Returns(user);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidCredentials_ReturnsToken()
    {
        var password = "senha123";
        var model = new LoginModel { Login = "joao@test.com", Password = password };
        var user = new Domain.User { Email = "joao@test.com", Password = HashHelper.HashGeneration(password), Status = EUserStatus.Ativo };
        _userRepo.GetWithInclude(
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>(),
            Arg.Any<Func<System.Linq.IQueryable<Domain.User>, System.Linq.IQueryable<Domain.User>>>())
            .Returns(user);
        _tokenService.GetToken(user).Returns(FactoryResponse<dynamic>.Success(new { AccessToken = "jwt_token" }));

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        await _tokenService.Received(1).GetToken(user);
    }
}
