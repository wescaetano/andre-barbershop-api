using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models
{
    public class FilterModel
    {
        [Display(Name = "Id da Entidade")]
        public long? Id { get; set; }

        [Display(Name = "Total de registros por página")]
        [DefaultValue(10)]
        public int? PageSize { get; set; } = 10;

        [Display(Name = "Página selecionada")]
        [DefaultValue(1)]
        public int? PageNumber { get; set; } = 1;

        [Display(Name = "Valor a ser filtrado")]
        public string Value { get; set; } = "";

        [Display(Name = "Ordenar por asc/desc")]
        [DefaultValue("asc")]
        public string SortOrder { get; set; } = "asc";

        [Required(ErrorMessage = "400")]
        [Display(Name = "Campo para ordenação")]
        [DefaultValue("Id")]
        public string SortField { get; set; } = "Id";
    }
}
