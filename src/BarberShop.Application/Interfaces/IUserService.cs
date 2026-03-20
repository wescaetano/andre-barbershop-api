using BarberShop.Application.Models.User;
using BarberShop.Communication.Filters;
using BarberShop.Communication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IUserService
    {
        Task<ResponseModel<dynamic>> Create(CreateUserModel model);
        Task<ResponseModel<dynamic>> Update(UpdateUserModel model);
        Task<ResponseModel<dynamic>> ChangeStatus(ChangeUserStatusModel model);
        Task<ResponseModel<dynamic>> GetById(long id);
        Task<ResponseModel<dynamic>> GetPaginated(UserFilterModel filter);
    }
}
