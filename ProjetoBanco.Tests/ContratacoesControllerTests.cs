using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Domain;
using ProjetoBanco.Api.DTOs;
using Xunit;

namespace ProjetoBanco.Tests;

public class ContratacoesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ContratacoesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(int clienteId, int produtoId)> SeedDados()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var agencia = new Agencia { Nome = "Ag Teste", Numero = "002", Endereco = "Rua B" };
        db.Agencias.Add(agencia);
        await db.SaveChangesAsync();

        var pf = new PessoaFisica
        {
            Nome = "Ana Lima",
            Email = "ana@email.com",
            Telefone = "11900001111",
            AgenciaId = agencia.Id,
            CPF = "11122233344",
            DataNascimento = new DateTime(1995, 8, 20)
        };
        db.PessoasFisicas.Add(pf);

        var emp = new Emprestimo
        {
            Nome = "Empréstimo Pessoal",
            Descricao = "Crédito pessoal",
            Tipo = "EMPRESTIMO",
            ValorSolicitado = 10000,
            TaxaJuros = 3.5m,
            PrazoDias = 360
        };
        db.Emprestimos.Add(emp);
        await db.SaveChangesAsync();

        return (pf.Id, emp.Id);
    }

    [Fact]
    public async Task SolicitarContratacao_Valida_Retorna202()
    {
        var (clienteId, produtoId) = await SeedDados();
        var dto = new CriarContratacaoDto(clienteId, produtoId);

        var response = await _client.PostAsJsonAsync("/api/contratacoes", dto);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task SolicitarContratacao_ClienteInexistente_Retorna404()
    {
        var (_, produtoId) = await SeedDados();
        var dto = new CriarContratacaoDto(99999, produtoId);

        var response = await _client.PostAsJsonAsync("/api/contratacoes", dto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ConsultarContratacao_Existente_RetornaStatus()
    {
        var (clienteId, produtoId) = await SeedDados();
        var criarResponse = await _client.PostAsJsonAsync("/api/contratacoes",
            new CriarContratacaoDto(clienteId, produtoId));

        var criada = await criarResponse.Content.ReadFromJsonAsync<ContratacaoResponse>();
        var getResponse = await _client.GetAsync($"/api/contratacoes/{criada!.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    private record ContratacaoResponse(int Id, string Status);
}