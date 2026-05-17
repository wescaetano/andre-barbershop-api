using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.Barber.GetAll
{
    public interface IGetBarbersUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(bool activeOnly);
    }
}
