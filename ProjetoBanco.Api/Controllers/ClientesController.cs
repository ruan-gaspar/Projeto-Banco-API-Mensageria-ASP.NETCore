using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Domain;
using ProjetoBanco.Api.DTOs;

namespace ProjetoBanco.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(AppDbContext db, ILogger<ClientesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost("pf")]
    public async Task<IActionResult> CadastrarPF([FromBody] CriarPessoaFisicaDto dto)
    {
        var agencia = await _db.Agencias.FindAsync(dto.AgenciaId);
        if (agencia is null)
            return NotFound(new { Erro = "Agência não encontrada." });

        // Usar FindAsync + FirstOrDefault para evitar AnyAsync (ORA-00904)
        var existe = await _db.PessoasFisicas
            .FirstOrDefaultAsync(p => p.CPF == dto.CPF);
        if (existe is not null)
            return BadRequest(new { Erro = "CPF já cadastrado." });

        var pf = new PessoaFisica
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            AgenciaId = dto.AgenciaId,
            CPF = dto.CPF,
            DataNascimento = dto.DataNascimento
        };

        _db.PessoasFisicas.Add(pf);
        await _db.SaveChangesAsync();
        _logger.LogInformation("PF {Nome} cadastrada com Id {Id}", pf.Nome, pf.Id);
        return CreatedAtAction(nameof(BuscarCliente), new { id = pf.Id }, pf);
    }

    [HttpPost("pj")]
    public async Task<IActionResult> CadastrarPJ([FromBody] CriarPessoaJuridicaDto dto)
    {
        var agencia = await _db.Agencias.FindAsync(dto.AgenciaId);
        if (agencia is null)
            return NotFound(new { Erro = "Agência não encontrada." });

        var existe = await _db.PessoasJuridicas
            .FirstOrDefaultAsync(p => p.CNPJ == dto.CNPJ);
        if (existe is not null)
            return BadRequest(new { Erro = "CNPJ já cadastrado." });

        var pj = new PessoaJuridica
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            AgenciaId = dto.AgenciaId,
            CNPJ = dto.CNPJ,
            RazaoSocial = dto.RazaoSocial
        };

        _db.PessoasJuridicas.Add(pj);
        await _db.SaveChangesAsync();
        _logger.LogInformation("PJ {RazaoSocial} cadastrada com Id {Id}", pj.RazaoSocial, pj.Id);
        return CreatedAtAction(nameof(BuscarCliente), new { id = pj.Id }, pj);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarCliente(int id)
    {
        var cliente = await _db.Clientes
            .Include(c => c.Agencia)
            .FirstOrDefaultAsync(c => c.Id == id);

        return cliente is null ? NotFound() : Ok(cliente);
    }
}