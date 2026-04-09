using Moq;
using Xunit;
using FluentAssertions;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.ValueObjects;
using Xunit.Abstractions;

namespace MinhasFinancas.Tests.Unit.Services;

public class TotalServiceTests
{
    private readonly Mock<ITotaisQuery> _queryMock;
    private readonly TotalService _service;
    private readonly ITestOutputHelper _output;

    public TotalServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _queryMock = new Mock<ITotaisQuery>();
        _service = new TotalService(_queryMock.Object);

        _output.WriteLine("[INFO] Suite de testes de unidade inicializada para TotalService.");
    }

    [Fact]
    [Trait("Prioridade", "P1")]
    [Trait("Tipo", "Integracao_Interna")]
    public async Task GetTotaisPorPessoaAsync_DeveChamarQueryCorretamente()
    {
        _output.WriteLine("[LOG] Validando o repasse de parâmetros e consumo da ITotaisQuery.");

        // Configuração de um retorno de página vazio para simular comportamento da Query
        var pagedResult = new PagedResult<TotalPorPessoa>(); 
        
        _queryMock.Setup(x => x.GetTotaisPorPessoaAsync(null, null))
                  .ReturnsAsync(pagedResult);

        _output.WriteLine("[DATA] Executando chamada sem filtros de paginação.");

        // Execução da chamada via serviço
        var resultado = await _service.GetTotaisPorPessoaAsync(null, null);

        // Validação se o serviço atua corretamente como mediador para a camada de infraestrutura/query
        resultado.Should().NotBeNull("o serviço deve retornar o PagedResult fornecido pela query, mesmo que vazio.");
        
        _queryMock.Verify(x => x.GetTotaisPorPessoaAsync(null, null), Times.Once, 
            "A query de totais deve ser acionada exatamente uma vez para evitar consumo excessivo de recursos.");

        _output.WriteLine("[LOG] Teste finalizado: Comunicação entre Service e Query validada.");
    }
}