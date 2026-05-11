namespace ProjetoBanco.Api.DTOs;

public record CriarPessoaFisicaDto(
    string Nome,
    string Email,
    string Telefone,
    int AgenciaId,
    string CPF,
    DateTime DataNascimento
);