using Moq;
using Xunit;
using FluentAssertions;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.Entities;
using Xunit.Abstractions;

namespace MinhasFinancas.Tests.Unit.Services;

public class TransacaoServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly TransacaoService _service;
    private readonly ITestOutputHelper _output;

    public TransacaoServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _uowMock = new Mock<IUnitOfWork>();
        _service = new TransacaoService(_uowMock.Object);
        
        _output.WriteLine("[INFO] Inicializando suite de testes de unidade para TransacaoService.");
    }

    // --- FLUXOS POSITIVOS (HAPPY PATH) ---

    [Fact]
    [Trait("Prioridade", "P0")]
    [Trait("Cobertura", "Funcional")]
    public async Task CreateAsync_DeveExecutarComSucesso_QuandoDadosForemValidos()
    {
        _output.WriteLine("[LOG] Validando persistência de transação com massa de dados íntegra.");

        // Configuração de estado válido para evitar gatilhos de regras de restrição
        var pessoaId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        var pessoa = new Pessoa { Id = pessoaId, Nome = "Usuário Teste", DataNascimento = DateTime.Now.AddYears(-25) };
        var categoria = new Categoria { Id = categoriaId, Descricao = "Alimentação" };
        
        var dto = new CreateTransacaoDto {
            PessoaId = pessoaId,
            CategoriaId = categoriaId,
            Valor = 150.50m,
            Data = DateTime.Now,
            Descricao = "Almoço Executivo"
        };

        _uowMock.Setup(x => x.Pessoas.GetByIdAsync(pessoaId)).ReturnsAsync(pessoa);
        _uowMock.Setup(x => x.Categorias.GetByIdAsync(categoriaId)).ReturnsAsync(categoria);

        var resultado = await _service.CreateAsync(dto);

        // Validação de integridade do retorno e persistência atômica
        resultado.Should().NotBeNull("o retorno do serviço deve conter o objeto DTO preenchido após a criação.");
        _uowMock.Verify(x => x.CommitAsync(), Times.Once, "O método Commit deve ser invocado exatamente uma vez para garantir a persistência.");
        
        _output.WriteLine("[LOG] Sucesso: Fluxo de criação concluído sem violações.");
    }

    // --- VALIDAÇÕES DE REGRAS DE NEGÓCIO E SEGURANÇA ---

    [Fact]
    [Trait("Prioridade", "P1")]
    [Trait("Regra", "RN04")]
    public async Task CreateAsync_DeveLancarExcecao_QuandoMenorDeIdadeTentaCriarReceita()
    {
        _output.WriteLine("[LOG] Validando restrição RN04: Bloqueio de lançamentos de receita para menores de 18 anos.");
        
        var pessoaId = Guid.NewGuid();
        var pessoaMenor = new Pessoa { Id = pessoaId, DataNascimento = DateTime.Now.AddYears(-15) };
        var categoria = new Categoria { Id = Guid.NewGuid(), Descricao = "Salário" };
        
        var dto = new CreateTransacaoDto { 
            PessoaId = pessoaId, 
            CategoriaId = categoria.Id, 
            Valor = 500.00m 
        };

        _uowMock.Setup(x => x.Pessoas.GetByIdAsync(pessoaId)).ReturnsAsync(pessoaMenor);
        _uowMock.Setup(x => x.Categorias.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(categoria);

        _output.WriteLine($"[DATA] Idade simulada: 15 anos. Categoria: {categoria.Descricao}.");

        // Verificação da barreira de validação de negócio na camada de serviço
        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Menores de 18 anos não podem ter receitas*", "o sistema deve impedir registros de rendimentos para este perfil etário.");
    }

    [Fact]
    [Trait("Prioridade", "P1")]
    [Trait("Regra", "RN01")]
    public async Task CreateAsync_DeveLancarExcecao_QuandoValorForZeroOuNegativo()
    {
        _output.WriteLine("[LOG] Validando integridade financeira: Rejeição de valores monetários inválidos.");
        
        var dto = new CreateTransacaoDto { Valor = -0.01m };

        // Teste de borda para garantir que valores não positivos não afetem o cálculo de saldo
        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<ArgumentException>("transações com valores zero ou negativos são consideradas inconsistentes.");
    }

    [Fact]
    [Trait("Prioridade", "P2")]
    [Trait("Regra", "Integridade_Referencial")]
    public async Task CreateAsync_DeveLancarExcecao_QuandoPessoaNaoExistir()
    {
        _output.WriteLine("[LOG] Validando resiliência do serviço perante chaves estrangeiras inexistentes.");
        
        var dto = new CreateTransacaoDto { PessoaId = Guid.NewGuid() };
        _uowMock.Setup(x => x.Pessoas.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Pessoa)null!);

        // Garante que o serviço não prossiga com a criação se a entidade relacionada for nula
        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<Exception>("o sistema deve validar a existência da entidade relacionada antes de processar a criação.");
            
        _output.WriteLine("[LOG] Teste de integridade referencial finalizado.");
    }
}