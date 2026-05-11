using System.ComponentModel.DataAnnotations;

namespace ProjetoBanco.Api.DTOs;

public record CriarPessoaJuridicaDto(
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
    [StringLength(14, MinimumLength = 14)]
    string CNPJ,

    [Required]
    [StringLength(150)]
    string RazaoSocial
);