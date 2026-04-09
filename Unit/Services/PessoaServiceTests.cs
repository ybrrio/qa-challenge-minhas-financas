using Moq;
using Xunit;
using FluentAssertions;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.Entities;
using Xunit.Abstractions;

namespace MinhasFinancas.Tests.Unit.Services;

public class PessoaServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly PessoaService _service;
    private readonly ITestOutputHelper _output;

    public PessoaServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _uowMock = new Mock<IUnitOfWork>();
        _service = new PessoaService(_uowMock.Object);
        
        _output.WriteLine("[INFO] Suite de testes de unidade inicializada para PessoaService.");
    }

    // --- VALIDAÇÕES DE ENTRADA E REGRAS DE DOMÍNIO ---

    [Fact]
    [Trait("Prioridade", "P1")]
    [Trait("Tipo", "Validacao")]
    public async Task CreateAsync_DeveLancarExcecao_QuandoNomeForVazio()
    {
        _output.WriteLine("[LOG] Validando obrigatoriedade do campo 'Nome' no cadastro de pessoas.");

        // Massa de teste com string vazia para validar barreira de entrada
        var dto = new CreatePessoaDto { Nome = "", DataNascimento = DateTime.Now.AddYears(-20) };

        // Verificação se o serviço impede a persistência de registros sem identificação
        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<ArgumentException>("o sistema não deve permitir o cadastro de pessoas com nome vazio.");
    }

    [Fact]
    [Trait("Prioridade", "P1")]
    [Trait("Tipo", "Sanidade")]
    public async Task CreateAsync_DeveLancarExcecao_QuandoDataNascimentoNoFuturo()
    {
        _output.WriteLine("[LOG] Validando regra de sanidade cronológica: Data de nascimento futura.");

        // Massa de teste configurada com data posterior à atual
        var dto = new CreatePessoaDto { Nome = "João Silva", DataNascimento = DateTime.Now.AddYears(1) };

        // O sistema deve validar que a data de nascimento é coerente com o presente
        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<ArgumentException>("o sistema deve rejeitar datas de nascimento que ainda não ocorreram.");
    }

    // --- TESTES DE CONSULTA E RESILIÊNCIA ---

    [Fact]
    [Trait("Prioridade", "P2")]
    [Trait("Tipo", "Integridade")]
    public async Task GetByIdAsync_DeveRetornarNull_QuandoPessoaNaoExistir()
    {
        _output.WriteLine("[LOG] Verificando tratamento de busca por ID inexistente.");

        var idInexistente = Guid.NewGuid();
        
        // Simulação de ausência de registro no repositório de pessoas
        _uowMock.Setup(x => x.Pessoas.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Pessoa)null!);

        var resultado = await _service.GetByIdAsync(idInexistente);

        // O comportamento esperado é um retorno nulo limpo, sem disparar exceções técnicas (ex: NullReference)
        resultado.Should().BeNull("a ausência de um registro deve ser tratada como um retorno nulo, não como erro de sistema.");
        
        _output.WriteLine("[LOG] Resiliência confirmada: Sistema lidou corretamente com ID não encontrado.");
    }
}