using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models.Report
{
    public class CsvTransactionRowModel
    {
        /// <summary>
        /// Data da transação
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Tipo da transação (INCOME / EXPENSE)
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Nome da categoria
        /// </summary>
        public string Category { get; set; } = null!;

        /// <summary>
        /// Descrição da transação
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Valor da transação
        /// Entradas positivas, saídas negativas
        /// </summary>
        public decimal Value { get; set; }
    }
}
