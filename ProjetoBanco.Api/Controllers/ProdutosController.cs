using Microsoft.AspNetCore.Mvc;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Domain;

namespace ProjetoBanco.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProdutosController(AppDbContext db) => _db = db;

    [HttpPost("emprestimo")]
    public async Task<IActionResult> CriarEmprestimo([FromBody] CriarEmprestimoDto dto)
    {
        var emp = new Emprestimo
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Tipo = "EMPRESTIMO",
            ValorSolicitado = dto.ValorSolicitado,
            TaxaJuros = dto.TaxaJuros,
            PrazoDias = dto.PrazoDias
        };
        _db.Emprestimos.Add(emp);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Buscar), new { id = emp.Id }, emp);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var produto = await _db.Produtos.FindAsync(id);
        return produto is null ? NotFound() : Ok(produto);
    }
}

public record CriarEmprestimoDto(
    string Nome,
    string Descricao,
    decimal ValorSolicitado,
    decimal TaxaJuros,
    int PrazoDias
);