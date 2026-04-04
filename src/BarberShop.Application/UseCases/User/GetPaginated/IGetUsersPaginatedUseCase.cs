using BarberShop.Application.Models.User;
using BarberShop.Communication.Models;

namespace BarberShop.Application.UseCases.User.GetPaginated
{
    public interface IGetUsersPaginatedUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(GetUsersPaginatedModel model);
    }
}
