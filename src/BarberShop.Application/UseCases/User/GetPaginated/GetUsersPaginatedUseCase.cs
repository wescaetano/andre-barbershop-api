using BarberShop.Application.Models.User;
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Application.UseCases.User.GetPaginated
{
    public class GetUsersPaginatedUseCase : IGetUsersPaginatedUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepository;

        public GetUsersPaginatedUseCase(IBaseRepository<Domain.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(GetUsersPaginatedModel model)
        {
            var validator = new GetUsersPaginatedValidator();
            var validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return FactoryResponse<dynamic>.InvalidModel(errors);
            }

            var query = _userRepository.GetAllWithInclude(
                filter: null,
                setIncludes: q => q
                    .Include(u => u.ProfilesUsers)
                    .ThenInclude(pu => pu.Profile)
            );

            // Filters
            if (!string.IsNullOrWhiteSpace(model.Name))
                query = query.Where(u => u.Name.ToLower().Contains(model.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(model.Email))
                query = query.Where(u => u.Email.ToLower().Contains(model.Email.ToLower()));

            if (model.Status.HasValue)
                query = query.Where(u => u.Status == model.Status.Value);

            var total = query.LongCount();

            // Sorting
            query = (model.SortField.ToLower(), model.SortOrder.ToLower()) switch
            {
                ("name", "desc")         => query.OrderByDescending(u => u.Name),
                ("name", _)              => query.OrderBy(u => u.Name),
                ("email", "desc")        => query.OrderByDescending(u => u.Email),
                ("email", _)             => query.OrderBy(u => u.Email),
                ("creationdate", "desc") => query.OrderByDescending(u => u.CreationDate),
                ("creationdate", _)      => query.OrderBy(u => u.CreationDate),
                (_, "desc")              => query.OrderByDescending(u => u.Id),
                _                        => query.OrderBy(u => u.Id)
            };

            // Pagination
            var skip = (model.PageNumber - 1) * model.PageSize;

            var users = await Task.FromResult(
                query
                    .Skip(skip)
                    .Take(model.PageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.Name,
                        u.Email,
                        u.ImageUrl,
                        Status = (int)u.Status,
                        u.CreationDate,
                        Profiles = u.ProfilesUsers.Select(pu => new
                        {
                            pu.ProfileId,
                            Name = pu.Profile!.Name
                        })
                    })
                    .ToList<object>()
            );

            var pagination = new PaginationData<object>(users, model.PageNumber, model.PageSize, total);

            return FactoryResponse<dynamic>.Success(pagination);
        }
    }
}
