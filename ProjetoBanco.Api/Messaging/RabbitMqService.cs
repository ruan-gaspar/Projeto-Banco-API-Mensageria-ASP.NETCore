using RabbitMQ.Client;
using System.Text;

namespace ProjetoBanco.Api.Messaging;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqService(IConfiguration config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
            UserName = config["RabbitMQ:User"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publicar(string fila, string mensagem)
    {
        _channel.QueueDeclare(queue: fila, durable: true, exclusive: false, autoDelete: false);
        var body = Encoding.UTF8.GetBytes(mensagem);
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        _channel.BasicPublish(exchange: "", routingKey: fila, basicProperties: props, body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}