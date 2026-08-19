using Estoque.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<OperacaoEstoque> OperacoesEstoque => Set<OperacaoEstoque>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(produto => produto.Id);

                entity.HasIndex(produto => produto.Codigo)
                    .IsUnique();

                entity.Property(produto => produto.Codigo)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(produto => produto.Descricao)
                    .IsRequired()
                    .HasMaxLength(200);

                // Adiciona a restrição de verificação para garantir que o saldo não seja negativo
                entity.ToTable(table =>
                    table.HasCheckConstraint(
                        "CK_Produtos_Saldo_NaoNegativo",
                        "\"Saldo\" >= 0"));
            });

            modelBuilder.Entity<OperacaoEstoque>(entity =>
            {
                entity.ToTable("OperacoesEstoque");

                entity.HasKey(operacao =>
                    operacao.ChaveIdempotencia);

                entity.Property(operacao =>
                        operacao.ChaveIdempotencia)
                    .HasMaxLength(100);

                entity.Property(operacao =>
                        operacao.DataDeProcessamento)
                    .IsRequired();
            });
        }
    }
}