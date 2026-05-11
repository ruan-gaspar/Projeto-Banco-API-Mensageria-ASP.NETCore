namespace ProjetoBanco.Api.Domain;

public class Contratacao
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProdutoId { get; set; }
    public string Status { get; set; } = StatusContratacao.Pendente;
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataProcessamento { get; set; }
    public string Observacao { get; set; } = string.Empty;
    public Cliente Cliente { get; set; } = null!;
    public Produto Produto { get; set; } = null!;
}