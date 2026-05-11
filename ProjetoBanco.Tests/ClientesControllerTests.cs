using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Domain;
using ProjetoBanco.Api.DTOs;
using Xunit;

namespace ProjetoBanco.Tests;

public class ClientesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ClientesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<Agencia> CriarAgencia()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agencia = new Agencia { Nome = "Ag Central", Numero = "001", Endereco = "Rua A" };
        db.Agencias.Add(agencia);
        await db.SaveChangesAsync();
        return agencia;
    }

    [Fact]
    public async Task CadastrarPF_Valido_Retorna201()
    {
        var agencia = await CriarAgencia();
        var dto = new CriarPessoaFisicaDto(
            "João Silva", "joao@email.com", "11999990000",
            agencia.Id, "12345678901", new DateTime(1990, 1, 1));

        var response = await _client.PostAsJsonAsync("/api/clientes/pf", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CadastrarPF_CPFDuplicado_Retorna400()
    {
        var agencia = await CriarAgencia();
        var dto = new CriarPessoaFisicaDto(
            "Maria Souza", "maria@email.com", "11888880000",
            agencia.Id, "99988877766", new DateTime(1985, 5, 15));

        await _client.PostAsJsonAsync("/api/clientes/pf", dto);
        var response = await _client.PostAsJsonAsync("/api/clientes/pf", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CadastrarPJ_Valido_Retorna201()
    {
        var agencia = await CriarAgencia();
        var dto = new CriarPessoaJuridicaDto(
            "Empresa X", "contato@empresa.com", "1133334444",
            agencia.Id, "12345678000199", "Empresa X Ltda");

        var response = await _client.PostAsJsonAsync("/api/clientes/pj", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CadastrarPJ_CNPJDuplicado_Retorna400()
    {
        var agencia = await CriarAgencia();
        var dto = new CriarPessoaJuridicaDto(
            "Empresa Y", "y@empresa.com", "1133335555",
            agencia.Id, "98765432000100", "Empresa Y SA");

        await _client.PostAsJsonAsync("/api/clientes/pj", dto);
        var response = await _client.PostAsJsonAsync("/api/clientes/pj", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CadastrarCliente_AgenciaInexistente_Retorna404()
    {
        var dto = new CriarPessoaFisicaDto(
            "Carlos", "carlos@email.com", "11777770000",
            9999, "55544433322", new DateTime(2000, 3, 10));

        var response = await _client.PostAsJsonAsync("/api/clientes/pf", dto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}