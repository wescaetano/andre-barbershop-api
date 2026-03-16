using BarberShop.Application.Interfaces;
using BarberShop.Domain;
using BarberShop.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyScope.Application.Services
{
    public class BaseService : IBaseService
    {
        private readonly IRepositoryFactory _repositoryFactory;
        public BaseService(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }
        public IBaseRepository<T> _repository<T>() where T : BaseEntity
        {
            return _repositoryFactory.GetRepository<T>();
        }
        public IBaseRelationRepository<T> _relationRepository<T>() where T : BaseEntityRelation
        {
            return _repositoryFactory.GetRepositoryRelation<T>();
        }
    }
}
