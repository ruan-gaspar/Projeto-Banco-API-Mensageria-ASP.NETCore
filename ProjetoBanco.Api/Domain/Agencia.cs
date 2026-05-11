namespace ProjetoBanco.Api.Domain;

public class Agencia
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}