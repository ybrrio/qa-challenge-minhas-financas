using Moq;
using Xunit;
using FluentAssertions;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.Entities;
using Xunit.Abstractions;

namespace MinhasFinancas.Tests.Unit.Services;

public class CategoriaServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CategoriaService _service;
    private readonly ITestOutputHelper _output;

    public CategoriaServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _uowMock = new Mock<IUnitOfWork>();
        _service = new CategoriaService(_uowMock.Object);

        _output.WriteLine("[INFO] Suite de testes de unidade inicializada para CategoriaService.");
    }

    // --- VALIDAÇÕES DE REGRAS DE DOMÍNIO ---

    [Fact]
    [Trait("Prioridade", "P1")]
    [Trait("Tipo", "Validacao")]
    public async Task CreateAsync_DeveLancarExcecao_QuandoDescricaoForVazia()
    {
        _output.WriteLine("[LOG] Validando obrigatoriedade do campo 'Descricao' na criação de categoria.");

        // Massa de teste configurada com descrição vazia para validar barreira de entrada
        var dto = new CreateCategoriaDto 
        { 
            Descricao = "", 
            Finalidade = Categoria.EFinalidade.Receita 
        };

        // O sistema deve interceptar a ausência de descrição antes da persistência
        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<ArgumentException>("a descrição da categoria é um campo obrigatório para a classificação financeira.");
        
        _output.WriteLine("[LOG] Validação de campo obrigatório verificada com sucesso.");
    }

    // --- TESTES DE BUSCA E RESILIÊNCIA ---

    [Fact]
    [Trait("Prioridade", "P2")]
    [Trait("Tipo", "Integridade")]
    public async Task GetByIdAsync_DeveRetornarNull_QuandoCategoriaNaoExistir()
    {
        _output.WriteLine("[LOG] Verificando comportamento do sistema para consulta de ID inexistente.");

        // Simulação de retorno nulo pelo repositório (registro não encontrado)
        _uowMock.Setup(x => x.Categorias.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Categoria)null!);

        var resultado = await _service.GetByIdAsync(Guid.NewGuid());

        // Verificação se o serviço retorna nulo graciosamente, evitando falhas catastróficas
        resultado.Should().BeNull("consultas por IDs inexistentes não devem disparar exceções, apenas retornar nulo.");
        
        _output.WriteLine("[LOG] Resiliência confirmada: Busca por ID inválido tratada corretamente.");
    }
}