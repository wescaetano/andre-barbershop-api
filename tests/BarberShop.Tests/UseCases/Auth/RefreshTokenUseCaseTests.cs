using BarberShop.Application.Interfaces;
using BarberShop.Application.UseCases.Auth.RefreshToken;
using BarberShop.Communication.Models;
using NSubstitute;

namespace BarberShop.Tests.UseCases.Auth;

public class RefreshTokenUseCaseTests
{
    private readonly ITokenService _tokenService;
    private readonly RefreshTokenUseCase _sut;

    public RefreshTokenUseCaseTests()
    {
        _tokenService = Substitute.For<ITokenService>();
        _sut = new RefreshTokenUseCase(_tokenService);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyToken_ReturnsBadRequest()
    {
        var result = await _sut.ExecuteAsync(string.Empty);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        await _tokenService.DidNotReceive().ValidateRefreshToken(Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceToken_ReturnsBadRequest()
    {
        var result = await _sut.ExecuteAsync("   ");

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidToken_ReturnsUnauthorized()
    {
        _tokenService.ValidateRefreshToken(Arg.Any<string>())
            .Returns((ResponseModel<dynamic>?)null);

        var result = await _sut.ExecuteAsync("invalid-token");

        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidToken_ReturnsNewAccessToken()
    {
        var newTokenResponse = FactoryResponse<dynamic>.Success(new { AccessToken = "new-jwt", RefreshToken = "new-refresh" });

        _tokenService.ValidateRefreshToken(Arg.Any<string>())
            .Returns(newTokenResponse);

        var result = await _sut.ExecuteAsync("valid-refresh-token");

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        await _tokenService.Received(1).ValidateRefreshToken("valid-refresh-token");
    }
}
