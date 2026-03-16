using BarberShop.Domain;
using BarberShop.Infra.DataAccess;
using BarberShop.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infra.Repositories
{
    public class BaseRelationRepository<T> : IBaseRelationRepository<T> where T : BaseEntityRelation
    {
        protected readonly BarberShopContext _dataContext;
        public BaseRelationRepository(BarberShopContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<T> Create(T entity)
        {
            _dataContext.Add(entity);
            await _dataContext.SaveChangesAsync();
            return entity;
        }
        public async Task<List<T>> Create(List<T> entities)
        {
            await _dataContext.AddRangeAsync(entities);

            await _dataContext.SaveChangesAsync();
            return entities;
        }
        public async Task<List<T>> Get()
        {
            var obj = await _dataContext.Set<T>()
               .AsNoTracking().ToListAsync();
            return obj;
        }
        public async Task<T?> Get(Expression<Func<T, bool>> expression, bool asNoTracking = true) =>
            await _dataContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(expression);
        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> expression) =>
            await _dataContext.Set<T>().AsNoTracking().Where(expression)
                     .ToListAsync();
        public async Task Remove(T obj)
        {
            _dataContext.Set<T>().Remove(obj);

            await _dataContext.SaveChangesAsync();
        }
        public async Task RemoveRange(List<T> entities)
        {
            _dataContext.RemoveRange(entities);
            await _dataContext.SaveChangesAsync();
        }
        public async Task<List<T>> AddRange(List<T> entities)
        {
            await _dataContext.AddRangeAsync(entities);

            await _dataContext.SaveChangesAsync();
            return entities;
        }
        public async Task<List<T>> UpdateRange(List<T> entities)
        {
            _dataContext.UpdateRange(entities);

            await _dataContext.SaveChangesAsync();
            return entities;
        }
        public IQueryable<T> GetAllWithInclude(Expression<Func<T, bool>>? filtro, Func<IQueryable<T>, IQueryable<T>>? configurarIncludes = null)
        {
            var query = _dataContext.Set<T>().AsNoTracking();
            if (configurarIncludes != null)
            {
                query = configurarIncludes(query);
            }
            if (filtro != null) query = query.Where(filtro);
            return query;
        }
        public async Task<T?> GetWithInclude(Expression<Func<T, bool>>? filtro, Func<IQueryable<T>, IQueryable<T>>? configurarIncludes = null)
        {
            var query = _dataContext.Set<T>().AsNoTracking();
            if (configurarIncludes != null)
            {
                query = configurarIncludes(query);
            }
            if (filtro != null) query = query.Where(filtro);
            return await query.FirstOrDefaultAsync();
        }
    }
}
