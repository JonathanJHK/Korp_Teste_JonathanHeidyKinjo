using Faturamento.Api.Clients;
using Faturamento.Api.Data;
using Faturamento.Api.Interfaces;
using Faturamento.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurando a conexão com o banco de dados PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IEstoqueClient, EstoqueClient>(
    (serviceProvider, httpClient) =>
    {
        // Configurando o endereço do serviço de Estoque
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        // Configurando o endereço do serviço de Estoque
        var baseUrl = configuration[
            "Services:EstoqueApi:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "O endereço do serviço de Estoque não foi configurado.");
        }

        httpClient.BaseAddress = new Uri(baseUrl);

        httpClient.Timeout = TimeSpan.FromSeconds(5);
    });

builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
