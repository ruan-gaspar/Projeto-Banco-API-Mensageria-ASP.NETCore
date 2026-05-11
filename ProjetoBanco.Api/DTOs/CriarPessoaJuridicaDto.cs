namespace ProjetoBanco.Api.DTOs;

public record CriarPessoaJuridicaDto(
    string Nome,
    string Email,
    string Telefone,
    int AgenciaId,
    string CNPJ,
    string RazaoSocial
);