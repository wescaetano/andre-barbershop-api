
using BarberShop.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infra.Interfaces
{
    public interface IBaseRelationRepository<T> where T : BaseEntityRelation
    {
        Task<T> Create(T entity);
        Task<List<T>> Create(List<T> entities);
        Task Remove(T obj);
        Task<List<T>> Get();
        Task<T?> Get(Expression<Func<T, bool>> expression, bool asNoTracking = true);
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> expression);
        Task<T?> GetWithInclude(Expression<Func<T, bool>>? filtro, Func<IQueryable<T>, IQueryable<T>>? configurarIncludes = null);
        IQueryable<T> GetAllWithInclude(Expression<Func<T, bool>>? filtro, Func<IQueryable<T>, IQueryable<T>>? configurarIncludes = null);
        Task<List<T>> UpdateRange(List<T> entities);
        Task<List<T>> AddRange(List<T> entities);
        Task RemoveRange(List<T> entities);
    }
}
