using Estoque.Api.Data;
using Estoque.Api.DTOs;
using Estoque.Api.Entities;
using Estoque.Api.Exceptions;
using Estoque.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Estoque.Api.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly AppDbContext _appDbContext;

        public ProdutoService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<ProdutoResponseDTO> Cadastrar(
            ProdutoCriarDTO novoProduto,
            CancellationToken cancellationToken)
        {
            // Normaliza o código do produto para evitar duplicatas devido a diferenças de maiúsculas/minúsculas ou espaços em branco. 
            var codigoNormalizado = novoProduto.Codigo.Trim().ToUpperInvariant();

            // Verifica se já existe um produto com o mesmo código no banco de dados.
            var codigoJaCadastrado = await _appDbContext.Produtos.AnyAsync(
                produto => produto.Codigo == codigoNormalizado,
                cancellationToken);

            if (codigoJaCadastrado)
            {
                throw new CodigoProdutoDuplicadoException(codigoNormalizado);
            }

            // Cria uma nova entidade Produto com os dados fornecidos.
            var produto = new Produto
            {
                Codigo = codigoNormalizado,
                Descricao = novoProduto.Descricao.Trim(),
                Saldo = novoProduto.Saldo ?? 0, // Se o saldo for nulo, define como 0
                DataDeCadastro = DateTime.UtcNow
            };

            // Adiciona o novo produto ao contexto do banco de dados.
            _appDbContext.Produtos.Add(produto);

            try
            {
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) // Captura exceções de atualização do banco de dados
            when (exception.InnerException is PostgresException // Verifica se a exceção interna é uma PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation // Verifica se o código de erro SQL corresponde a uma violação de chave única
            })
            {
                // Se ocorrer uma violação de chave única, lança uma exceção personalizada.
                throw new CodigoProdutoDuplicadoException(codigoNormalizado);
            }

            // Retorna um DTO de resposta com os dados do produto recém-criado.
            return new ProdutoResponseDTO
            {
                Id = produto.Id,
                Codigo = produto.Codigo,
                Descricao = produto.Descricao,
                Saldo = produto.Saldo,
                DataDeCadastro = produto.DataDeCadastro
            };

        }

        public async Task<IReadOnlyList<ProdutoResponseDTO>> Listar(
            CancellationToken cancellationToken)
        {
            return await _appDbContext.Produtos
                .AsNoTracking()
                .OrderBy(produto => produto.Codigo)
                .Select(produto => new ProdutoResponseDTO
                {
                    Id = produto.Id,
                    Codigo = produto.Codigo,
                    Descricao = produto.Descricao,
                    Saldo = produto.Saldo,
                    DataDeCadastro = produto.DataDeCadastro
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<ProdutoResponseDTO> BuscarPorId(
            int id,
            CancellationToken cancellationToken)
        {
            // Busca o produto com o ID fornecido no banco de dados.
            var produto = await _appDbContext.Produtos
                .AsNoTracking()
                .Where(produto => produto.Id == id)
                .Select(produto => new ProdutoResponseDTO
                {
                    Id = produto.Id,
                    Codigo = produto.Codigo,
                    Descricao = produto.Descricao,
                    Saldo = produto.Saldo,
                    DataDeCadastro = produto.DataDeCadastro
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (produto is null)
            {
                throw new ProdutoNaoEncontradoException(id);
            }

            return produto;
        }

    }
}