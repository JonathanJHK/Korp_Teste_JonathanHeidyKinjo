namespace Estoque.Api.Entities
{
    public class Produto
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public int Saldo { get; set; }

        public DateTime DataDeCadastro { get; set; } = DateTime.UtcNow;
    }
}