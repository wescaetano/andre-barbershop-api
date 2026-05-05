using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.UseCases.Auth.ResetPasswwordById
{
    public interface IResetPasswordByIdUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(ResetPasswordByIdModel model);
    }
}
