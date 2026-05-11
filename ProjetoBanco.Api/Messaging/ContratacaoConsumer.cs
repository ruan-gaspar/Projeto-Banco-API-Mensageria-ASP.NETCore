using ProjetoBanco.Api.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Services;

namespace ProjetoBanco.Api.Messaging;

public class ContratacaoConsumer : BackgroundService
{
    private readonly ILogger<ContratacaoConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel? _channel;
    private const string Fila = "contratacao-solicitada";

    public ContratacaoConsumer(
        ILogger<ContratacaoConsumer> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _config = config;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
                UserName = _config["RabbitMQ:User"] ?? "guest",
                Password = _config["RabbitMQ:Password"] ?? "guest",
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: Fila, durable: true, exclusive: false, autoDelete: false);
            _channel.BasicQos(0, 1, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ indisponível no startup — Consumer não iniciado");
            return Task.CompletedTask; // Não derruba o host
        }
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            _logger.LogWarning("Consumer não iniciado pois RabbitMQ estava indisponível.");
            return Task.CompletedTask;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var mensagem = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Mensagem recebida: {Mensagem}", mensagem);

            try
            {
                var payload = JsonSerializer.Deserialize<ContratacaoPayload>(mensagem);
                if (payload is null) throw new Exception("Payload inválido");

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var emprestimoService = scope.ServiceProvider.GetRequiredService<EmprestimoService>();

                var contratacao = await db.Contratacoes
                    .Include(c => c.Produto)
                    .FirstOrDefaultAsync(c => c.Id == payload.ContratacaoId, stoppingToken);

                if (contratacao is null)
                {
                    _logger.LogWarning("Contratação {Id} não encontrada", payload.ContratacaoId);
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                // Regra de negócio (para dupla: lógica extra de score/taxa)
                var (aprovado, obs) = emprestimoService.ProcessarEmprestimo(contratacao);

                contratacao.Status = aprovado
                    ? StatusContratacao.Aprovada
                    : StatusContratacao.Recusada;

                contratacao.Observacao = obs;
                contratacao.DataProcessamento = DateTime.UtcNow;

                await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Contratação {Id} processada: {Status}", contratacao.Id, contratacao.Status);

                //apenas para teste com RabbitMQ 
                //await Task.Delay(30000, stoppingToken);

                _channel!.BasicAck(ea.DeliveryTag, false); // ACK manual após sucesso
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar contratação — mensagem volta para a fila");
                _channel!.BasicNack(ea.DeliveryTag, false, true); // NACK: requeue = true
            }
        };

        _channel!.BasicConsume(queue: Fila, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

public record ContratacaoPayload(int ContratacaoId, string TipoProduto);