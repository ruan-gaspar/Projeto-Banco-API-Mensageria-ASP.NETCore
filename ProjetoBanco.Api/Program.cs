using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Messaging;
using ProjetoBanco.Api.Services;

using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var oracleUser = Environment.GetEnvironmentVariable("ORACLE_USER");
var oraclePassword = Environment.GetEnvironmentVariable("ORACLE_PASSWORD");
var oracleHost = Environment.GetEnvironmentVariable("ORACLE_HOST");
var oraclePort = Environment.GetEnvironmentVariable("ORACLE_PORT");
var oracleService = Environment.GetEnvironmentVariable("ORACLE_SERVICE");

var oracleConnectionString =
    $"User Id={oracleUser};" +
    $"Password={oraclePassword};" +
    $"Data Source={oracleHost}:{oraclePort}/{oracleService};";

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/banco-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Oracle EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnectionString));

// Health Checks (sem Oracle em ambiente Testing)
var hcBuilder = builder.Services.AddHealthChecks();

if (!builder.Environment.IsEnvironment("Testing"))
{
    hcBuilder.AddDbContextCheck<AppDbContext>();
}

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ProjetoBanco"))
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

// RabbitMQ e Consumer
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddHostedService<ContratacaoConsumer>();

// Services
builder.Services.AddScoped<EmprestimoService>();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            Erro = "Erro interno no servidor."
        });
    });
});
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Expõe o Program para WebApplicationFactory nos testes
public partial class Program { }