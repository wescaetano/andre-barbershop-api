using BarberShop.Communication.Enums.User;

namespace BarberShop.Application.Models.User
{
    public class GetUsersPaginatedModel
    {
        /// <summary>Filtra por nome (busca parcial, case-insensitive)</summary>
        public string? Name { get; set; }

        /// <summary>Filtra por e-mail (busca parcial, case-insensitive)</summary>
        public string? Email { get; set; }

        /// <summary>Filtra por status do usuário (Ativo / Inativo)</summary>
        public EUserStatus? Status { get; set; }

        /// <summary>Quantidade de registros por página (padrão: 20, máximo: 100)</summary>
        public int PageSize { get; set; } = 20;

        /// <summary>Número da página (começa em 1)</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Campo para ordenação: Id | Name | Email | CreationDate (padrão: Id)</summary>
        public string SortField { get; set; } = "Id";

        /// <summary>Direção de ordenação: asc | desc (padrão: asc)</summary>
        public string SortOrder { get; set; } = "asc";
    }
}
