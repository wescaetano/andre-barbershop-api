using BarberShop.Application.Interfaces;
using BarberShop.Application.Models.User;
using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Application.UseCases.User.Create;
using BarberShop.Communication.Models;
using BarberShop.Domain;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;
using NSubstitute;

namespace BarberShop.Tests.UseCases.User;

public class CreateUserUseCaseTests
{
    private readonly IBaseRepository<Domain.User> _userRepo;
    private readonly IBaseRelationRepository<ProfileUser> _profileUserRepo;
    private readonly IBlobService _blobService;
    private readonly ISendEmailResetPasswordUseCase _sendEmailUseCase;
    private readonly CreateUserUseCase _sut;

    public CreateUserUseCaseTests()
    {
        _userRepo = Substitute.For<IBaseRepository<Domain.User>>();
        _profileUserRepo = Substitute.For<IBaseRelationRepository<ProfileUser>>();
        _blobService = Substitute.For<IBlobService>();
        _sendEmailUseCase = Substitute.For<ISendEmailResetPasswordUseCase>();
        _sut = new CreateUserUseCase(_userRepo, _profileUserRepo, _blobService, _sendEmailUseCase);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidModel_ReturnsAllValidationErrors()
    {
        var model = new CreateUserModel { Name = "", Email = "nao-e-email", AccessProfile = 0 };

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(406, result.StatusCode);
        Assert.Contains("nome", result.Description);
        Assert.Contains("email", result.Description);
        Assert.Contains("perfil", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_EmailAlreadyExists_ReturnsConflict()
    {
        var model = new CreateUserModel { Name = "João", Email = "joao@test.com", AccessProfile = 1 };
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>())
            .Returns(new Domain.User { Email = "joao@test.com" });

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ProfileNotFound_ReturnsNotFound()
    {
        var model = new CreateUserModel { Name = "João", Email = "joao@test.com", AccessProfile = 99 };
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>())
            .Returns((Domain.User?)null);
        _profileUserRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<ProfileUser, bool>>>())
            .Returns((ProfileUser?)null);

        var result = await _sut.ExecuteAsync(model);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModel_CreatesUserAndSendsEmail()
    {
        var model = new CreateUserModel { Name = "João", Email = "joao@test.com", AccessProfile = 1 };
        _userRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.User, bool>>>())
            .Returns((Domain.User?)null);
        _profileUserRepo.Get(Arg.Any<System.Linq.Expressions.Expression<Func<ProfileUser, bool>>>())
            .Returns(new ProfileUser { ProfileId = 1 });
        _userRepo.Create(Arg.Any<Domain.User>()).Returns(x => x.ArgAt<Domain.User>(0));
        _sendEmailUseCase.ExecuteAsync(Arg.Any<string>())
            .Returns(FactoryResponse<dynamic>.Success("Email enviado."));

        var result = await _sut.ExecuteAsync(model);

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        await _userRepo.Received(1).Create(Arg.Any<Domain.User>());
        await _sendEmailUseCase.Received(1).ExecuteAsync(model.Email);
    }
}
