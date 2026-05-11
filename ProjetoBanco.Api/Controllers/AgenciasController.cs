using Microsoft.AspNetCore.Mvc;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Domain;
using ProjetoBanco.Api.DTOs;

namespace ProjetoBanco.Api.Controllers;

[ApiController]
[Route("api/agencias")]
public class AgenciasController : ControllerBase
{
    private readonly AppDbContext _db;

    public AgenciasController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarAgenciaDto dto)
    {
        var agencia = new Agencia
        {
            Nome = dto.Nome,
            Numero = dto.Numero,
            Endereco = dto.Endereco
        };
        _db.Agencias.Add(agencia);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Buscar), new { id = agencia.Id }, agencia);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var agencia = await _db.Agencias.FindAsync(id);
        return agencia is null ? NotFound() : Ok(agencia);
    }
}