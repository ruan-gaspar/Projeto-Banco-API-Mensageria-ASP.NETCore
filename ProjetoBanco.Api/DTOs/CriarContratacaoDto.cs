using System.ComponentModel.DataAnnotations;

namespace ProjetoBanco.Api.DTOs;

public record CriarContratacaoDto(
    [Required]
    int ClienteId,

    [Required]
    int ProdutoId
);