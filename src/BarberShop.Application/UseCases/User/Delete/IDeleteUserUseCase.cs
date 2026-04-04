using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.User.Delete
{
    public interface IDeleteUserUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long id);
    }
}
