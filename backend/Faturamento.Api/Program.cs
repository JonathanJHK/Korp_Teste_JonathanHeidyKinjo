using Faturamento.Api.Clients;
using Faturamento.Api.Data;
using Faturamento.Api.Exceptions;
using Faturamento.Api.Interfaces;
using Faturamento.Api.Services;
using Microsoft.EntityFrameworkCore;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Configurando a conexão com o banco de dados PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Registra o cliente tipado usado para comunicação com a API de Estoque.
builder.Services.AddHttpClient<IEstoqueClient, EstoqueClient>(
        (serviceProvider, httpClient) =>
        {
            // Obtém a configuração da aplicação para localizar a API de Estoque.
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Lê o endereço base configurado para as requisições do cliente.
            var baseUrl = configuration["Services:EstoqueApi:BaseUrl"];

            // Interrompe a inicialização se o serviço não tiver um endereço válido.
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("O endereço do serviço de Estoque não foi configurado.");
            }

            // Define a URL base que será usada pelo EstoqueClient.
            httpClient.BaseAddress = new Uri(baseUrl);

            // Deixa o controle de tempo limite a cargo do pipeline de resiliência.
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
        })
    // Adiciona políticas automáticas para lidar com falhas temporárias e lentidão.
    .AddStandardResilienceHandler(options =>
    {
        // Repete requisições que falharem até três vezes.
        options.Retry.MaxRetryAttempts = 3;

        // Aguarda inicialmente 500 ms entre as tentativas.
        options.Retry.Delay = TimeSpan.FromMilliseconds(500);

        // Aumenta progressivamente o intervalo entre as novas tentativas.
        options.Retry.BackoffType = DelayBackoffType.Exponential;

        // Adiciona uma variação aleatória aos intervalos para evitar picos simultâneos.
        options.Retry.UseJitter = true;

        // Limita o tempo permitido para cada tentativa individual.
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);

        // Limita o tempo total de todas as tentativas da requisição.
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(12);

        // Abre o circuito quando pelo menos metade das chamadas recentes falhar.
        options.CircuitBreaker.FailureRatio = 0.5;

        // Exige quatro chamadas antes de avaliar a taxa de falhas.
        options.CircuitBreaker.MinimumThroughput = 4;

        // Considera as falhas observadas dentro de uma janela de 30 segundos.
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);

        // Mantém o circuito aberto por 10 segundos antes de permitir novas chamadas.
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);
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
