namespace ProjetoBanco.Api.Domain;

public class Emprestimo : Produto
{
    public decimal ValorSolicitado { get; set; }
    public decimal TaxaJuros { get; set; }
    public int PrazoDias { get; set; }
}