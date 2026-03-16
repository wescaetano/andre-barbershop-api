using BarberShop.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Infra.Interfaces
{
    public interface IRepositoryFactory
    {
        IBaseRepository<T> GetRepository<T>() where T : BaseEntity;
        IBaseRelationRepository<T> GetRepositoryRelation<T>() where T : BaseEntityRelation;
    }
}
