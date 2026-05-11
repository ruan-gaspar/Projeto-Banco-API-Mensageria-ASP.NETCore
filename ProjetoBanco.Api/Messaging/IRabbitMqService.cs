namespace ProjetoBanco.Api.Messaging;

public interface IRabbitMqService
{
    void Publicar(string fila, string mensagem);
}