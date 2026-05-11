namespace ProjetoBanco.Api.Domain;

public class PessoaJuridica : Cliente
{
    public string CNPJ { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
}