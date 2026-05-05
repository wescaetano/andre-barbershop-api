using BarberShop.Application.Interfaces;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Auth;
using BarberShop.Communication.Utils;
using BarberShop.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.UseCases.Auth.ResetPasswwordById
{
    public class ResetPasswordByIdUseCase : IResetPasswordByIdUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;

        public ResetPasswordByIdUseCase(IBaseRepository<Domain.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(ResetPasswordByIdModel model)
        {
            try
            {
                var user = await _userRepository.Get(u => u.Id == model.UserId);
                if (user == null) return FactoryResponse<dynamic>.NotFound("Usuário não encontrado");

                user.Password = HashHelper.HashGeneration(model.NewPassword);
                await _userRepository.Update(user);
                return FactoryResponse<dynamic>.Success("Senha alterada com sucesso!");
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
            
        }
    }
}
