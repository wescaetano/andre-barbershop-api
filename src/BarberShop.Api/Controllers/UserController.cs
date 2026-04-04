using BarberShop.Api.Authorization;
using BarberShop.Application.Models.User;
using BarberShop.Application.UseCases.User.ChangeStatus;
using BarberShop.Application.UseCases.User.Create;
using BarberShop.Application.UseCases.User.Delete;
using BarberShop.Application.UseCases.User.GetById;
using BarberShop.Application.UseCases.User.GetPaginated;
using BarberShop.Application.UseCases.User.Update;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Gerenciamento de usuários</summary>
    [APIAuthorization("Users-C", "Users-E", "Users-V", "Users-I")]
    public class UserController : BaseController
    {
        private readonly ICreateUserUseCase _createUserUseCase;
        private readonly IUpdateUserUseCase _updateUserUseCase;
        private readonly IChangeUserStatusUseCase _changeUserStatusUseCase;
        private readonly IGetUserByIdUseCase _getUserByIdUseCase;
        private readonly IGetUsersPaginatedUseCase _getUsersPaginatedUseCase;
        private readonly IDeleteUserUseCase _deleteUserUseCase;

        /// <summary></summary>
        public UserController(
            ICreateUserUseCase createUserUseCase,
            IUpdateUserUseCase updateUserUseCase,
            IChangeUserStatusUseCase changeUserStatusUseCase,
            IGetUserByIdUseCase getUserByIdUseCase,
            IGetUsersPaginatedUseCase getUsersPaginatedUseCase,
            IDeleteUserUseCase deleteUserUseCase)
        {
            _createUserUseCase = createUserUseCase;
            _updateUserUseCase = updateUserUseCase;
            _changeUserStatusUseCase = changeUserStatusUseCase;
            _getUserByIdUseCase = getUserByIdUseCase;
            _getUsersPaginatedUseCase = getUsersPaginatedUseCase;
            _deleteUserUseCase = deleteUserUseCase;
        }

        /// <summary>Cria um novo usuário</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserModel model)
        {
            var result = await _createUserUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Atualiza dados de um usuário existente</summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateUserModel model)
        {
            var result = await _updateUserUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Altera o status (ativo/inativo) de um usuário</summary>
        [HttpPatch("status")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeUserStatusModel model)
        {
            var result = await _changeUserStatusUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Busca um usuário pelo ID, incluindo seus perfis de acesso</summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            var result = await _getUserByIdUseCase.ExecuteAsync(id);
            return Result(result);
        }

        /// <summary>Lista usuários com paginação e filtros. Filtros: name, email, status. Ordenação: Id | Name | Email | CreationDate (asc/desc)</summary>
        [HttpGet]
        public async Task<IActionResult> GetPaginated([FromQuery] GetUsersPaginatedModel model)
        {
            var result = await _getUsersPaginatedUseCase.ExecuteAsync(model);
            return Result(result);
        }

        /// <summary>Remove um usuário via soft delete (preenche ExclusionDate, não apaga do banco)</summary>
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var result = await _deleteUserUseCase.ExecuteAsync(id);
            return Result(result);
        }
    }
}
