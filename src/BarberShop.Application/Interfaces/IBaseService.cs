using BarberShop.Domain;
using BarberShop.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IBaseService
    {
        IBaseRepository<T> _repository<T>() where T : BaseEntity;
        IBaseRelationRepository<T> _relationRepository<T>() where T : BaseEntityRelation;
    }
}
