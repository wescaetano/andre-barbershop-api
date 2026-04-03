using BarberShop.Application.Interfaces;
using BarberShop.Application.UseCases.Auth.ResetPassword;
using BarberShop.Communication.Models.Auth;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Auth;

public class ResetPasswordUseCaseTests
{
    private readonly IBaseRepository<Domain.User> _userRepo;
    private readonly ITokenService _tokenService;
    private readonly ResetPasswordUseCase _sut;

    public ResetPasswordUseCaseTests()
    {
        _userRepo = Substitute.For<IBaseRepository<Domain.User>>();
        _tokenService = Substitute.For<ITokenService>();
        _sut = new ResetPasswordUseCase(_userRepo, _tokenService);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyFields_ReturnsAllValidationErrors()
    {
        var model = new ResetPasswordModel { Token = "", NewPassword = "" };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidToken_ReturnsForbidden()
    {
        var model = new ResetPasswordModel { Token = "token_invalido", NewPassword = "novaSenha123" };
        _tokenService.IsValidToken(model.Token).Returns(false);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidToken_UserNotFound_ReturnsNotFound()
    {
        var model = new ResetPasswordModel { Token = "token_valido", NewPassword = "novaSenha123" };
        _tokenService.IsValidToken(model.Token).Returns(true);
        _tokenService.Getclaim("unique_name", model.Token).Returns("joao@test.com");
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>())
            .Returns((Domain.User?)null);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidToken_ResetsPassword()
    {
        var model = new ResetPasswordModel { Token = "token_valido", NewPassword = "novaSenha123" };
        var user = new Domain.User { Id = 1, Email = "joao@test.com" };
        _tokenService.IsValidToken(model.Token).Returns(true);
        _tokenService.Getclaim("unique_name", model.Token).Returns("joao@test.com");
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>()).Returns(user);
        _userRepo.Update(Arg.Any<Domain.User>()).Returns(user);

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        await _userRepo.Received(1).Update(Arg.Any<Domain.User>());
    }
}
