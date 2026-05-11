using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Domain;
using ProjetoBanco.Api.DTOs;
using ProjetoBanco.Api.Messaging;

namespace ProjetoBanco.Api.Controllers;

[ApiController]
[Route("api/contratacoes")]
public class ContratacoesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IRabbitMqService _rabbit;
    private readonly ILogger<ContratacoesController> _logger;

    public ContratacoesController(
        AppDbContext db,
        IRabbitMqService rabbit,
        ILogger<ContratacoesController> logger)
    {
        _db = db;
        _rabbit = rabbit;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Solicitar([FromBody] CriarContratacaoDto dto)
    {
        _logger.LogInformation(
        "Solicitando contratação ClienteId={ClienteId} ProdutoId={ProdutoId}",
        dto.ClienteId,
        dto.ProdutoId);

        var cliente = await _db.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null)
            return NotFound(new { Erro = "Cliente não encontrado." });

        var produto = await _db.Produtos.FindAsync(dto.ProdutoId);
        if (produto is null)
            return NotFound(new { Erro = "Produto não encontrado." });

        var contratacao = new Contratacao
        {
            ClienteId = dto.ClienteId,
            ProdutoId = dto.ProdutoId,
            Status = StatusContratacao.Pendente,
            DataSolicitacao = DateTime.UtcNow
        };

        _db.Contratacoes.Add(contratacao);
        await _db.SaveChangesAsync();

        var payload = JsonSerializer.Serialize(new ContratacaoPayload(contratacao.Id,ObterTipoProduto(produto)));
        _rabbit.Publicar("contratacao-solicitada", payload);

        _logger.LogInformation("Contratação {Id} publicada na fila", contratacao.Id);
        return Accepted(new { contratacao.Id, contratacao.Status });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ConsultarStatus(int id)
    {
        var contratacao = await _db.Contratacoes
            .Include(c => c.Cliente)
            .Include(c => c.Produto)
            .FirstOrDefaultAsync(c => c.Id == id);

        return contratacao is null ? NotFound() : Ok(contratacao);
    }

    private static string ObterTipoProduto(Produto produto)
    {
        return produto switch
        {
            Emprestimo => "EMPRESTIMO",
            MaquinaDeCartao => "MAQUINA",
            ReceberSalario => "SALARIO",
            _ => "DESCONHECIDO"
        };
    }
}