using Microsoft.Extensions.Logging;
using Moq;
using ProjetoBanco.Api.Domain;
using ProjetoBanco.Api.Services;
using Xunit;

namespace ProjetoBanco.Tests;

public class EmprestimoServiceTests
{
    [Fact]
    public void ProcessarEmprestimo_DeveAprovar()
    {
        var logger = new Mock<ILogger<EmprestimoService>>();
        var service = new EmprestimoService(logger.Object);

        var emprestimo = new Emprestimo
        {
            ValorSolicitado = 10000,
            TaxaJuros = 3,
            PrazoDias = 360
        };

        var contratacao = new Contratacao
        {
            Produto = emprestimo
        };

        var resultado = service.ProcessarEmprestimo(contratacao);

        Assert.True(resultado.Aprovado);
    }

    [Fact]
    public void ProcessarEmprestimo_DeveRecusar()
    {
        var logger = new Mock<ILogger<EmprestimoService>>();
        var service = new EmprestimoService(logger.Object);

        var emprestimo = new Emprestimo
        {
            ValorSolicitado = 500000,
            TaxaJuros = 90,
            PrazoDias = 30
        };

        var contratacao = new Contratacao
        {
            Produto = emprestimo
        };

        var resultado = service.ProcessarEmprestimo(contratacao);

        Assert.False(resultado.Aprovado);
    }
}