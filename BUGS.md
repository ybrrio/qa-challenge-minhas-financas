# Relatório de Defeitos (Bug Report)

Este documento centraliza os achados técnicos, falhas de regras de negócio e instabilidades identificadas durante a execução da suite de testes automatizados.

-----------------------------------------------------------------------------------------------------------

## [BUG-001] Falha na Validação de Regra de Negócio: Idade mínima para Receitas
**Status:** Aberto  
**Severidade:** Crítica  
**Prioridade:** Alta  

### Descrição
O sistema não aplica a restrição de idade para o cadastro de transações do tipo "Receita". Segundo os requisitos do projeto, menores de 18 anos estão impedidos de possuir receitas. O sistema permite o processamento até a camada de serviço, onde ocorre um erro inesperado de execução.

### Cenário de Teste
1. Instanciar objeto `Pessoa` com idade de 15 anos.
2. Criar `DTO` de transação configurado como "Receita".
3. Executar o método `CreateAsync` no `TransacaoService`.

### Resultado Esperado
O sistema deve interceptar a operação e retornar uma exceção do tipo `ArgumentException` com a mensagem: *"Pessoas menores de 18 anos não podem possuir receitas."*

### Resultado Obtido
O sistema ignora a validação de idade e interrompe a execução com uma `NullReferenceException` na camada de aplicação.

### Evidências Técnicas (Log do Teste)
text
[FAIL] MinhasFinancas.Tests.TransacaoServiceTests.CreateAsync_DeveLancarExcecao_QuandoMenorDeIdadeTentaCriarReceita
Expected a <System.ArgumentException> to be thrown because Menores de 18 anos não podem ter receitas., but found <System.NullReferenceException>
at MinhasFinancas.Application.Services.TransacaoService.CreateAsync(CreateTransacaoDto dto) in .../TransacaoService.cs:line 98 

-----------------------------------------------------------------------------------------------------------

### [BUG-002] Falha Crítica: NullReferenceException no fluxo principal (Happy Path)
**Status:** Aberto
**Severidade:** Crítica
**Prioridade:** Alta

## Descrição
O método CreateAsync falha ao processar transações mesmo quando todos os dados fornecidos são 100% válidos. Este comportamento indica uma falha estrutural na camada de serviço, impedindo o funcionamento básico da aplicação (Caminho Feliz).

## Cenário de Teste
1. Instanciar uma Pessoa adulta válida.
2. Fornecer uma Categoria existente.
3. Chamar o método CreateAsync com valores positivos e datas válidas.

## Resultado Esperado
A transação deve ser persistida com sucesso e o método deve retornar os dados da transação criada.

## Resultado Obtido
O sistema encerra a execução prematuramente com um erro de referência nula.

## Evidências Técnicas (Log do Teste)
Plaintext
[FAIL]MinhasFinancas.Tests.TransacaoServiceTests.CreateAsync_DeveExecutarComSucesso_QuandoDadosForemValidos
System.NullReferenceException : Object reference not set to an instance of an object.
at MinhasFinancas.Application.Services.TransacaoService.CreateAsync(CreateTransacaoDto dto) in .../TransacaoService.cs:line 98

-----------------------------------------------------------------------------------------------------------

## [BUG-003] Ausência de Validação de Valores Negativos

**Status:** Aberto
**Severidade:** Alta
**Prioridade:** Média

## Descrição
O sistema não possui filtros ou travas para valores monetários negativos ou zerados. A aplicação tenta processar valores financeiros inválidos, resultando em um erro técnico (Crash) em vez de retornar uma mensagem de validação tratada para o usuário.

## Cenário de Teste
1. Criar um DTO de transação com o campo Valor contendo um número negativo (ex: -100).
2. Chamar o método CreateAsync.

##Resultado Esperado
O sistema deve validar o campo e lançar uma ArgumentException informando que o valor deve ser maior que zero.

## Resultado Obtido
O sistema tenta processar o valor e gera uma exceção técnica de referência nula na linha 80.

Evidências Técnicas (Log do Teste)
Expected a <System.ArgumentException> to be thrown because O valor da transação deve ser positivo, but found <System.NullReferenceException>
at MinhasFinancas.Application.Services.TransacaoService.CreateAsync(CreateTransacaoDto dto) in .../TransacaoService.cs:line 80

-----------------------------------------------------------------------------------------------------------

## [BUG-004] Ausência de Validação de Nome e Data no PessoaService
**Status:** Aberto
**Severidade:** Crítica

## Descrição
O PessoaService.cs não valida as propriedades do DTO. O sistema permite que a execução chegue à camada de persistência com o campo Nome vazio ou DataNascimento no futuro, causando falhas catastróficas (Crashes).

## Análise de Código
O método CreateAsync realiza apenas a checagem do objeto DTO (ArgumentNullException.ThrowIfNull(dto)), mas falha em validar o conteúdo interno, resultando em erro na linha 82 ao tentar salvar as alterações.

## Evidências Técnicas (Log do Teste)
[FAIL] CreateAsync_DeveLancarExcecao_QuandoNomeForVazio
Expected a <System.ArgumentException> but found <System.NullReferenceException>
at MinhasFinancas.Application.Services.PessoaService.CreateAsync(CreatePessoaDto dto) in .../PessoaService.cs:line 82

-----------------------------------------------------------------------------------------------------------

## [BUG-005] Defeito de Regra de Negócio: Aceite de Datas Futuras
**Status:** Aberto
**Severidade:** Alta

## Descrição
Não existe verificação de sanidade para datas de nascimento. O sistema aceita datas posteriores à data atual, o que viola a integridade dos dados cadastrais de usuários.

## Evidências Técnicas (Log do Teste)
[FAIL] CreateAsync_DeveLancarExcecao_QuandoDataNascimentoNoFuturo
Expected a <System.ArgumentException> but found <System.NullReferenceException>
at MinhasFinancas.Application.Services.PessoaService.CreateAsync(CreatePessoaDto dto) 

----------------------------------------------------------------------------------------------------------
## [BUG-007] Falha de Validação: CategoriaService (Descrição Vazia)
**Status:** Aberto  
**Severidade:** Crítica  

### Descrição
O `CategoriaService.cs` falha ao tentar processar categorias com descrição vazia. Em vez de retornar um erro de validação de negócio, o sistema sofre um crash por referência nula.

### Evidências Técnicas
[FAIL] CreateAsync_DeveLancarExcecao_QuandoDescricaoForVazia
Expected a <System.ArgumentException> but found <System.NullReferenceException>
at MinhasFinancas.Application.Services.CategoriaService.CreateAsync(CreateCategoriaDto dto) in .../CategoriaService.cs:line 81