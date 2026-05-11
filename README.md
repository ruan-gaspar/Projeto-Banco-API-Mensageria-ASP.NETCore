# Projeto Banco — API com Mensageria

> Backend de um banco digital construído com ASP.NET Core 8, Entity Framework Core, Oracle, RabbitMQ e OpenTelemetry.

---

## 1. Identificação

| Nome                                    | RM           |
|-----------------------------------------|--------------|
| _(Integrante 1 — Ruan Nunes Gaspar)_    | _(RM559567)_ |
| _(Integrante 2 — Rodrigo Paes Morales)_ | _(RM560209)_ |

---

## 2. Produto Bancário Escolhido

**Produto:** Empréstimo (`Emprestimo`)

**Justificativa:** O empréstimo foi escolhido por exigir a regra de negócio mais rica entre os três produtos disponíveis. Além da validação básica de campos (valor, taxa, prazo), foi implementado um **algoritmo de score financeiro** (requisito de dupla) que avalia três dimensões ponderadas:

- **Score de taxa de juros** — penaliza taxas acima de 15% a.a.
- **Score de prazo** — favorece contratos de longo prazo (≥ 360 dias)
- **Score de valor solicitado** — limita exposição a valores acima de R$ 200.000

Contratações com score total ≥ 60 são **APROVADAS**; abaixo disso, **RECUSADAS** com justificativa detalhada no campo `Observacao`.

---

## 3. Decisão de Modelagem de Filas

**Abordagem escolhida:** fila única `contratacao-solicitada` com discriminator `TipoProduto` no payload JSON.

**Trade-off considerado:**

| Critério | Fila única | N filas por produto |
|---|---|---|
| Complexidade de infraestrutura | Baixa — 1 fila para declarar | Alta — 1 fila por produto |
| Escalabilidade independente | Não (consumers disputam a fila) | Sim (cada fila tem seu consumer) |
| Simplicidade do Consumer | Necessita `switch` no payload | Consumer dedicado por produto |
| Adequação ao escopo | Projeto com 1 produto ativo | Melhor para 3+ produtos em produção |

Para um projeto individual/dupla com apenas 1 produto implementado na API, a fila única reduz overhead de infraestrutura sem perda funcional. O campo `TipoProduto` no payload permite extensão futura sem alteração de infraestrutura.

---

## 4. Diagrama de Classes

![Diagrama de Classes](./docs/diagrama-classes.png)

> Gerado a partir de `docs/diagrama-classes.puml`. Para regenerar: `plantuml docs/diagrama-classes.puml`

**Resumo do modelo:**
- `Cliente` é abstrata com herança TPH (tabela única) via discriminator `"Tipo"`: `PF` e `PJ`
- `Produto` é abstrata com herança TPH: `EMPRESTIMO`, `MAQUINA`, `SALARIO`
- `Agencia` possui N clientes (`1:N`)
- `Cliente` possui N contratações (`1:N`)
- `Contratacao` referencia 1 cliente e 1 produto

---

## 5. Como Rodar Localmente

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/)
- Credenciais Oracle FIAP (RM individual)

### 1. Subir o RabbitMQ via Docker

```bash
docker-compose up -d
```

Aguarde ~10 segundos e acesse o painel de gerenciamento:
**http://localhost:15672** — usuário: `guest` / senha: `guest`

### 2. Configurar a connection string Oracle

Crie um arquivo `.env` e copie o conteúdo de `.env.example`. Altere seu user e senha do banco Oracle:

```
ORACLE_USER=RM_SEUNUMERO
ORACLE_PASSWORD=SUA_SENHA
ORACLE_HOST=oracle.fiap.com.br
ORACLE_PORT=1521
ORACLE_SERVICE=ORCL

RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
```

### 3. Aplicar as Migrations

```bash
cd ProjetoBanco.Api
dotnet ef database update
```

### 4. Rodar a API

```bash
dotnet run
```

A API estará disponível em:
- Swagger: **http://localhost:5000/swagger**
- Health Check: **http://localhost:5000/health**

---

## 6. Endpoints Disponíveis

### POST `/api/clientes/pf` — Cadastra Pessoa Física

**Request:**
```json
{
  "nome": "João Silva",
  "email": "joao@email.com",
  "telefone": "11999990000",
  "agenciaId": 1,
  "cpf": "12345678901",
  "dataNascimento": "1990-01-15T00:00:00"
}
```

**Response 201:**
```json
{
  "id": 1,
  "nome": "João Silva",
  "email": "joao@email.com",
  "cpf": "12345678901",
  "agenciaId": 1
}
```

---

### POST `/api/clientes/pj` — Cadastra Pessoa Jurídica

**Request:**
```json
{
  "nome": "Empresa X",
  "email": "contato@empresax.com",
  "telefone": "1133334444",
  "agenciaId": 1,
  "cnpj": "12345678000199",
  "razaoSocial": "Empresa X Ltda"
}
```

**Response 201:**
```json
{
  "id": 2,
  "nome": "Empresa X",
  "cnpj": "12345678000199",
  "razaoSocial": "Empresa X Ltda",
  "agenciaId": 1
}
```

---

### GET `/api/clientes/{id}` — Busca Cliente por ID

**Response 200:**
```json
{
  "id": 1,
  "nome": "João Silva",
  "email": "joao@email.com",
  "agencia": {
    "id": 1,
    "nome": "Agência Central",
    "numero": "001"
  }
}
```

---

### POST `/api/agencias` — Cadastra Agência

**Request:**
```json
{
  "nome": "Agência Central",
  "numero": "001",
  "endereco": "Av. Paulista, 1000 — São Paulo/SP"
}
```

**Response 201:**
```json
{
  "id": 1,
  "nome": "Agência Central",
  "numero": "001",
  "endereco": "Av. Paulista, 1000 — São Paulo/SP"
}
```

---

### GET `/api/agencias/{id}` — Busca Agência por ID

**Response 200:**
```json
{
  "id": 1,
  "nome": "Agência Central",
  "numero": "001",
  "endereco": "Av. Paulista, 1000 — São Paulo/SP"
}
```

---

### POST `/api/contratacoes` — Solicita Contratação (publica na fila)

**Request:**
```json
{
  "clienteId": 1,
  "produtoId": 1
}
```

**Response 202:**
```json
{
  "id": 3,
  "status": "PENDENTE"
}
```

> A contratação é persistida com status `PENDENTE` e uma mensagem é publicada na fila `contratacao-solicitada`. O processamento ocorre de forma **assíncrona** pelo `ContratacaoConsumer`.

---

### GET `/api/contratacoes/{id}` — Consulta Status da Contratação

**Response 200 (após processamento):**
```json
{
  "id": 3,
  "status": "APROVADA",
  "observacao": "Empréstimo aprovado. Score: 80/100.",
  "dataSolicitacao": "2026-05-11T12:00:00Z",
  "dataProcessamento": "2026-05-11T12:00:02Z",
  "produto": {
    "tipo": "EMPRESTIMO",
    "valorSolicitado": 30000,
    "taxaJuros": 3.5,
    "prazoDias": 360
  }
}
```

---

### GET `/health` — Health Check

**Response 200:**
```json
{
  "status": "Healthy",
  "entries": {
    "AppDbContext": {
      "status": "Healthy"
    }
  }
}
```

---

## 7. Como Executar os Testes

```bash
cd ProjetoBanco.Tests
dotnet test --verbosity normal
```

### Fluxos cobertos

| Teste | Cenário |
|---|---|
| `CadastrarPF_Valido_Retorna201` | Cadastro de PF válida |
| `CadastrarPF_CPFDuplicado_Retorna400` | CPF já existente → 400 |
| `CadastrarPJ_Valido_Retorna201` | Cadastro de PJ válida |
| `CadastrarPJ_CNPJDuplicado_Retorna400` | CNPJ já existente → 400 |
| `CadastrarCliente_AgenciaInexistente_Retorna404` | Agência inválida → 404 |
| `SolicitarContratacao_Valida_Retorna202` | Contratação aceita e publicada na fila |
| `SolicitarContratacao_ClienteInexistente_Retorna404` | Cliente inexistente → 404 |
| `ConsultarContratacao_Existente_RetornaStatus` | Consulta de status via GET |

> Todos os testes usam `WebApplicationFactory<Program>` com Oracle substituído por banco InMemory e RabbitMQ mockado via Moq.

**Print do resultado:**
_(cole aqui o print do terminal com `dotnet test` após rodar)_

---

## 8. Painel do RabbitMQ

Acesse **http://localhost:15672** → aba **Queues** → fila `contratacao-solicitada`.

**Print do painel com mensagens processadas:**
_(cole aqui o print mostrando a fila com `Ready: 0` e `Total: N` após processar contratações)_

Para demonstrar o comportamento de **Unacked** (requisito do enunciado):
1. Adicione um `Thread.Sleep(30000)` temporário no `ContratacaoConsumer` antes do `BasicAck`
2. Envie uma contratação via POST
3. Tire o print do painel mostrando `Unacked: 1`
4. Remova o `Thread.Sleep`

---

## 9. Swagger com Contratação Aprovada

Acesse **http://localhost:5000/swagger**:

1. Execute `POST /api/agencias` para criar uma agência
2. Execute `POST /api/clientes/pf` para criar um cliente
3. Crie um produto diretamente no banco (via migrations seed ou SQL)
4. Execute `POST /api/contratacoes` com `clienteId` e `produtoId`
5. Aguarde ~2s e execute `GET /api/contratacoes/{id}` — status deverá ser `APROVADA`

**Print do Swagger com contratação aprovada:**
_(cole aqui o print da tela do Swagger mostrando a resposta do GET com `"status": "APROVADA"`)_

---

## 10. Stack Tecnológica

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 8.0 |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Banco | Oracle (`oracle.fiap.com.br:1521/ORCL`) |
| Logging | Serilog (console + arquivo em `logs/`) |
| Health Checks | `AspNetCore.HealthChecks.UI.Client` + `AddDbContextCheck` |
| Observabilidade | OpenTelemetry com exporter Console |
| Mensageria | RabbitMQ 3-management via Docker + `RabbitMQ.Client 6.8.1` |
| Testes | xUnit + Moq + `WebApplicationFactory<Program>` |

---

## 11. Estrutura de Pastas

```
ProjetoBanco/
├── docker-compose.yml
├── docs/
│   └── diagrama-classes.puml
├── ProjetoBanco.Api/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Controllers/
│   │   ├── ClientesController.cs
│   │   ├── AgenciasController.cs
│   │   └── ContratacoesController.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Domain/
│   │   ├── Cliente.cs
│   │   ├── PessoaFisica.cs
│   │   ├── PessoaJuridica.cs
│   │   ├── Agencia.cs
│   │   ├── Produto.cs
│   │   ├── Emprestimo.cs
│   │   ├── MaquinaDeCartao.cs
│   │   ├── ReceberSalario.cs
│   │   └── Contratacao.cs
│   ├── DTOs/
│   ├── Messaging/
│   │   ├── IRabbitMqService.cs
│   │   ├── RabbitMqService.cs
│   │   └── ContratacaoConsumer.cs
│   └── Services/
│       └── EmprestimoService.cs
└── ProjetoBanco.Tests/
    ├── CustomWebApplicationFactory.cs
    ├── ClientesControllerTests.cs
    └── ContratacoesControllerTests.cs
```