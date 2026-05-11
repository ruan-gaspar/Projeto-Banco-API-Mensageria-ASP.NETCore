namespace ProjetoBanco.Api.Domain;

public class PessoaFisica : Cliente
{
    public string CPF { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
}