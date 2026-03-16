using BarberShop.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infra.Interfaces
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<T> Create(T entity);
        Task<List<T>> Create(List<T> entities);
        Task<T> Update(T entity);
        Task Remove(T obj);
        Task<T?> Get(long id);
        Task<List<T>> Get();
        Task<T?> Get(Expression<Func<T, bool>> expression, bool asNoTracking = true);
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> expression);
        Task<T?> GetWithInclude(Expression<Func<T, bool>>? filter, Func<IQueryable<T>, IQueryable<T>>? setIncludes = null);
        IQueryable<T> GetAllWithInclude(Expression<Func<T, bool>>? filter, Func<IQueryable<T>, IQueryable<T>>? setIncludes = null);
        Task<List<T>> UpdateRange(List<T> entities);
        Task<List<T>> AddRange(List<T> entities);
        Task RemoveRange(List<T> entities);
    }
}
