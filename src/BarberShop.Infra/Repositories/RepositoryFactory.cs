using BarberShop.Domain;
using BarberShop.Infra.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyScope.Infra.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public RepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IBaseRepository<T> GetRepository<T>() where T : BaseEntity
        {
            return _serviceProvider.GetService<IBaseRepository<T>>()!;
        }
        public IBaseRelationRepository<T> GetRepositoryRelation<T>() where T : BaseEntityRelation
        {
            return _serviceProvider.GetService<IBaseRelationRepository<T>>()!;
        }
    }
}
