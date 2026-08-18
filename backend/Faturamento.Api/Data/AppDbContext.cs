using Faturamento.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
    {
        // Cria a tabela de notas fiscais
        public DbSet<NotaFiscal> NotasFiscais =>
       Set<NotaFiscal>();

        // Cria a tabela de itens de notas fiscais
        public DbSet<ItemNotaFiscal> ItensNotaFiscal =>
            Set<ItemNotaFiscal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Declaração da sequência, que será usada para gerar o número da nota fiscal
            modelBuilder
                .HasSequence<long>("numero_nota_fiscal")
                .StartsAt(1)
                .IncrementsBy(1);

            // Configuração da tabela de notas fiscais
            modelBuilder.Entity<NotaFiscal>(entity =>
            {
                entity.ToTable("NotasFiscais");

                entity.HasKey(nota => nota.Id);

                entity.HasIndex(nota => nota.Numero)
                    .IsUnique();

                // Impede chamadas concorrentes para a geração do número da nota fiscal
                entity.Property(nota => nota.Numero)
                    .HasDefaultValueSql(
                        "nextval('numero_nota_fiscal')")
                    .ValueGeneratedOnAdd();

                entity.Property(nota => nota.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(nota => nota.DataDeCriacao)
                    .IsRequired();

                entity.HasMany(nota => nota.Itens)
                    .WithOne()
                    .HasForeignKey(item => item.NotaFiscalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemNotaFiscal>(entity =>
            {
                entity.ToTable(
                    "ItensNotaFiscal",
                    table => table.HasCheckConstraint(
                        "CK_ItensNotaFiscal_Quantidade_Positiva",
                        "\"Quantidade\" > 0"));

                entity.HasKey(item => item.Id);

                entity.Property(item => item.ProdutoId)
                    .IsRequired();

                entity.Property(item => item.CodigoProduto)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(item => item.DescricaoProduto)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(item => item.Quantidade)
                    .IsRequired();
            });
        }
    }
}