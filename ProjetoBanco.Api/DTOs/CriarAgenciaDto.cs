using System.ComponentModel.DataAnnotations;

namespace ProjetoBanco.Api.DTOs;

public record CriarAgenciaDto(
    [Required]
    [StringLength(150)]
    string Nome,

    [Required]
    [StringLength(30)]
    string Numero,

    [Required]
    [StringLength(200)]
    string Endereco
);