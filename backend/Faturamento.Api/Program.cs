using Faturamento.Api.Clients;
using Faturamento.Api.Data;
using Faturamento.Api.Exceptions;
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
    // Configurando o HttpClient para o serviço de Estoque
    (serviceProvider, httpClient) =>
    {
        // Obtendo a instância do serviço IConfiguration
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        // Obtendo o endereço do serviço de Estoque a partir das configurações
        var baseUrl = configuration[
            "Services:EstoqueApi:BaseUrl"];

        // Verificando se o endereço do serviço de Estoque está configurado
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "O endereço do serviço de Estoque não foi configurado.");
        }

        // Definindo o endereço base do httpClient
        httpClient.BaseAddress = new Uri(baseUrl);

        // Definindo o tempo limite da requisição
        httpClient.Timeout = TimeSpan.FromSeconds(5);
    });

builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

// Configurando o tratamento global de exceções
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configurando o tratamento global de exceções
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Faturamento API");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
