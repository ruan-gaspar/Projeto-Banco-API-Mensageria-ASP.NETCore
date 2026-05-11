using System.ComponentModel.DataAnnotations;

namespace ProjetoBanco.Api.DTOs;

public record CriarPessoaFisicaDto(
    [Required]
    [StringLength(150)]
    string Nome,

    [Required]
    [EmailAddress]
    string Email,

    [Required]
    [Phone]
    string Telefone,

    [Required]
    int AgenciaId,

    [Required]
    [StringLength(11, MinimumLength = 11)]
    string CPF,

    [Required]
    DateTime DataNascimento
);