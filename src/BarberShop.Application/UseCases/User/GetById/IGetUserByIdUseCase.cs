using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.User.GetById
{
    public interface IGetUserByIdUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long id);
    }
}
