namespace ProjetoBanco.Api.Domain;

public abstract class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public ICollection<Contratacao> Contratacoes { get; set; } = new List<Contratacao>();
}