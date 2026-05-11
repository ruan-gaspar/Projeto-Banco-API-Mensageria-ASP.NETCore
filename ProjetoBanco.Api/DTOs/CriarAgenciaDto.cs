namespace ProjetoBanco.Api.DTOs;

public record CriarAgenciaDto(
    string Nome,
    string Numero,
    string Endereco
);