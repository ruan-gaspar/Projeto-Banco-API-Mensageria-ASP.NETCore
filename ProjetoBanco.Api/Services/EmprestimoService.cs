using ProjetoBanco.Api.Domain;

namespace ProjetoBanco.Api.Services;

public class EmprestimoService
{
    private readonly ILogger<EmprestimoService> _logger;

    public EmprestimoService(ILogger<EmprestimoService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Processa o empréstimo e retorna (aprovado, observacao).
    /// Regra extra (dupla): calcula score financeiro baseado em taxa e prazo.
    /// Score >= 60 = APROVADA.
    /// </summary>
    public (bool Aprovado, string Observacao) ProcessarEmprestimo(Contratacao contratacao)
    {
        if (contratacao.Produto is not Emprestimo emp)
            return (false, "Produto não é um empréstimo.");

        if (emp.ValorSolicitado <= 0)
            return (false, "Valor solicitado inválido.");

        if (emp.TaxaJuros <= 0 || emp.TaxaJuros > 100)
            return (false, "Taxa de juros fora do intervalo permitido (0-100%).");

        // Score: penaliza taxas altas e prazos curtos
        var scoreTaxa = emp.TaxaJuros <= 5 ? 40 : emp.TaxaJuros <= 15 ? 30 : 10;
        var scorePrazo = emp.PrazoDias >= 360 ? 40 : emp.PrazoDias >= 180 ? 30 : 20;
        var scoreValor = emp.ValorSolicitado <= 50000 ? 20 : emp.ValorSolicitado <= 200000 ? 15 : 5;

        var scoreTotal = scoreTaxa + scorePrazo + scoreValor;

        _logger.LogInformation(
            "Score calculado: {Score} (Taxa:{T} Prazo:{P} Valor:{V})",
            scoreTotal, scoreTaxa, scorePrazo, scoreValor);

        return scoreTotal >= 60
            ? (true, $"Empréstimo aprovado. Score: {scoreTotal}/100.")
            : (false, $"Empréstimo recusado. Score insuficiente: {scoreTotal}/100.");
    }
}