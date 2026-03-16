using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models
{
    public class PaginationData<T>
    {
        public IEnumerable<T> Data { get; set; }
        public long Total { get; set; }
        public long Page { get; set; } = 1;
        public long Limit { get; set; }
        public long Pages { get; set; } = 1;

        public PaginationData(IEnumerable<T> data, long? page, long? limit, long total)
        {
            Data = data;
            Total = total;
            Page = page ?? 1;
            Limit = limit ?? 30;

            this.SetPages(page);
        }

        public PaginationData(IEnumerable<T> data, long total, FilterModel model)
        {
            this.Data = data;
            this.Total = total;

            this.Page = model.PageNumber ?? 0;
            this.Limit = model.PageSize ?? 0;

            this.SetPages(this.Page);
        }

        private void SetPages(long? page)
        {
            if (this.Total > 1 && Page > 0 && Limit > 0)
            {
                var pages = Math.Ceiling((decimal.Parse(this.Total.ToString()) / decimal.Parse(Limit.ToString())));
                Pages = int.Parse(pages.ToString());
            }
            else
            {
                Page = page ?? 1;
            }
        }
    }
}
