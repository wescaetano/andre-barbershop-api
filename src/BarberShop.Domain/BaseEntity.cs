using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain
{
    public class BaseEntity
    {
        public long Id { get; set; }
        public DateTime? CreationDate { get; set; } = null;
        public DateTime? UpdateDate { get; set; }
        public DateTime? ExclusionDate { get; private set; }

        public void AddCreationDate()
        {
            CreationDate = DateTime.UtcNow;
        }

        public void AddUpdateDate()
        {
            UpdateDate = DateTime.UtcNow;
        }

        public void AddExclusionDate()
        {
            ExclusionDate = DateTime.UtcNow;
        }

        public void SetId(long id)
        {
            Id = id;
        }
    }
}
