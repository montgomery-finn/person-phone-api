# person-phone-api

API REST para cadastro de pessoas (`Person`) e dos telefones associados a elas
(`Phone`), construída em .NET seguindo arquitetura em camadas.

## Arquitetura em camadas

O código está dividido em 4 projetos, cada um com uma responsabilidade única:

- **`PersonPhone.Domain`** — entidades (`Person`, `Phone`), value objects (`Cpf`,
  `PhoneNumber`), enum `PhoneType` e interfaces de repositório (`IRepository<T>`,
  `IPersonRepository`, `IPhoneRepository`). Não depende de nenhum outro projeto nem de
  frameworks externos.
- **`PersonPhone.Application`** — casos de uso (`PersonService`, `PhoneService`), DTOs
  (`DTOs/Person`, `DTOs/Phone`) e validadores de entrada com FluentValidation
  (`Validators/Person`, `Validators/Phone`). Depende apenas de `Domain`.
- **`PersonPhone.Infrastructure`** — implementação dos repositórios
  (`InMemoryRepository<T>` genérico + `InMemoryPersonRepository`,
  `InMemoryPhoneRepository`), baseada em `ConcurrentDictionary`. Depende apenas de
  `Domain`.
- **`PersonPhone.Api`** — controllers ASP.NET Core (`PersonController`,
  `PhoneController`), composition root (`Program.cs`), tratamento global de erros e
  Swagger/OpenAPI. Depende de `Application`, `Infrastructure` e `Domain`.

A regra de dependência é sempre em direção ao `Domain`: nada no `Domain` conhece as
outras camadas, e a `Application` não conhece `Infrastructure` nem `Api` (a ligação
entre elas acontece só via injeção de dependência em `Program.cs`).

## Entidades e regras de negócio

### Person

| Campo | Regra |
|---|---|
| `Name` | Obrigatório. |
| `Cpf` | Value object (`Cpf`). Valida 11 dígitos, dígitos verificadores e rejeita sequências repetidas (ex: `111.111.111-11`). Deve ser único entre pessoas (checado por `IPersonRepository.ExistsByCpfAsync`, excluindo a própria pessoa em updates). |
| `BirthDate` | Deve estar no passado. |
| `IsActive` | Controla soft-delete via `Deactivate()`. Uma pessoa já inativa não pode ser atualizada nem excluída novamente (lança `ArgumentException`). |

### Phone

| Campo | Regra |
|---|---|
| `PersonId` | Obrigatório e deve apontar para uma pessoa **existente e ativa** (checado em `PhoneService.CreateAsync`). |
| `Type` | Enum `PhoneType`: `Mobile`, `Residential`, `Commercial`. |
| `Number` | Value object (`PhoneNumber`). Normaliza (remove caracteres não numéricos) e valida 10–11 dígitos. Não pode repetir para a **mesma** pessoa, mas pessoas diferentes podem ter o mesmo número (a unicidade é por `personId` + número, via `IPhoneRepository.ExistsByNumberAsync`). |
| `IsActive` | Controla soft-delete via `Deactivate()`. Um telefone já inativo não pode ser atualizado nem excluído novamente. |

### Value Objects (`Cpf`, `PhoneNumber`)

São `record`s imutáveis que validam e normalizam o valor recebido já no construtor
(lançando `ArgumentException` se inválido), expõem a versão normalizada em `.Value` e um
método estático `IsValid(string?)` usado pelos validadores do FluentValidation para
validar sem lançar exceção.

## DTOs (`Application/DTOs`)

| DTO | Propriedades |
|---|---|
| `CreatePersonRequest` | `Name`, `Cpf`, `BirthDate` |
| `UpdatePersonRequest` | `Name`, `Cpf`, `BirthDate` |
| `PersonResponse` | `Id`, `Name`, `Cpf`, `BirthDate`, `IsActive` |
| `CreatePhoneRequest` | `PersonId`, `Type`, `Number` |
| `UpdatePhoneRequest` | `Type`, `Number` |
| `PhoneResponse` | `Id`, `PersonId`, `Type`, `Number`, `IsActive` |

Convenção: DTOs de request nunca incluem `Id` nem `IsActive` — esses campos são
controlados internamente pelo domínio. DTOs de response sempre incluem os dois.

## Validação de entrada (FluentValidation)

Os validadores (`CreatePersonRequestDtoValidator`, `UpdatePersonRequestDtoValidator`,
`CreatePhoneRequestDtoValidator`, `UpdatePhoneRequestDtoValidator`) são registrados
automaticamente via assembly scanning em `Program.cs`
(`AddValidatorsFromAssemblyContaining<CreatePersonRequestDtoValidator>()`) e injetados
diretamente nos métodos dos controllers como `IValidator<T>`. A extensão
`ControllerBaseExtensions.ValidateAsync` roda a validação e, se houver erros, retorna um
`ValidationProblem` (400) antes de chamar o service.

Resumo das regras:

- **Person** (Create/Update): `Name` não vazio (máx. 100 caracteres); `Cpf` não vazio e
  válido (`Cpf.IsValid`); `BirthDate` obrigatória e no passado.
- **Phone** (Create): `PersonId` diferente de `Guid.Empty`; `Type` dentro do enum;
  `Number` não vazio e válido (`PhoneNumber.IsValid`).
- **Phone** (Update): mesmas regras de `Type` e `Number` (sem `PersonId`, que não é
  alterável).

## Tratamento de erros

`Program.cs` usa `UseExceptionHandler` para traduzir exceções não tratadas no mesmo
formato `ProblemDetails` que o FluentValidation já usa:

- `ArgumentException` (violação de regra de domínio, ex: CPF duplicado, pessoa
  inativa) → **400 Bad Request**.
- Qualquer outra exceção não tratada → **500 Internal Server Error**.

## Persistência

Os repositórios são in-memory (`ConcurrentDictionary` estático dentro de
`InMemoryRepository<T>`). Não há banco de dados real — os dados existem apenas durante a
execução do processo e são perdidos a cada reinício da aplicação.

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/person` | Cria uma pessoa |
| `GET` | `/person` | Lista todas as pessoas ativas |
| `GET` | `/person/{id}` | Busca uma pessoa por id |
| `PUT` | `/person/{id}` | Atualiza uma pessoa |
| `DELETE` | `/person/{id}` | Inativa (soft-delete) uma pessoa |
| `POST` | `/phone` | Cria um telefone |
| `GET` | `/phone?personId=` | Lista telefones ativos (filtro opcional por pessoa) |
| `GET` | `/phone/{id}` | Busca um telefone por id |
| `PUT` | `/phone/{id}` | Atualiza um telefone |
| `DELETE` | `/phone/{id}` | Inativa (soft-delete) um telefone |

## Requisitos e como executar

- **Pré-requisito**: [.NET 10 SDK](https://dotnet.microsoft.com/download) instalado
  (todos os projetos usam `TargetFramework net10.0`).
- **Rodar a API** (a partir da raiz do repositório):

  ```
  dotnet run --project src/PersonPhone.Api
  ```

- **URLs** (definidas em `src/PersonPhone.Api/Properties/launchSettings.json`):
  - perfil `http`: `http://localhost:5196`
  - perfil `https`: `https://localhost:7112` e `http://localhost:5196`
- **Swagger**: em ambiente de desenvolvimento
  (`ASPNETCORE_ENVIRONMENT=Development`, já configurado nos perfis de launch), a
  interface do Swagger fica disponível em `http://localhost:5196/swagger`, servindo o
  schema OpenAPI gerado em `/openapi/v1.json`.
- **Testar via arquivo `.http`**: `src/PersonPhone.Api/PersonPhone.Api.http` já contém
  requisições prontas para todos os endpoints, incluindo o fluxo feliz e casos de erro
  de validação/regra de negócio (CPF duplicado, `PersonId` inexistente ou inativo,
  número inválido, `Type` fora do enum). Basta abrir o arquivo no VS Code com a extensão
  **REST Client** (ou o suporte nativo a `.http`) e clicar em "Send Request" acima de
  cada bloco `###` — a variável `@HostAddress` já aponta para o perfil `http`
  (`localhost:5196`).

## Como rodar os testes

```
dotnet test
```

## Decisões técnicas

### Por que não foi usado AutoMapper

O mapeamento entidade → DTO de resposta (`ToResponse`, presente em `PersonService` e
`PhoneService`) é feito manualmente, por decisão deliberada — não é um descuido a ser
corrigido depois.

A partir da versão 15, o AutoMapper passou a exigir uma licença comercial (RPL 1.5).
Existe um tier gratuito para empresas/indivíduos com receita anual abaixo de US$5
milhões, mas mesmo nesse tier é necessário criar conta em automapper.io, gerar uma
license key e configurá-la (via `cfg.LicenseKey = "..."` ou variável de ambiente
`AUTOMAPPER_LICENSE_KEY`) apenas para fins de auditoria. A última versão sem essa
exigência (licença MIT, `12.0.1`) não recebe atualizações desde 2022.

Dado o escopo pequeno do projeto (2 entidades, 1 método de mapeamento cada, sem lógica
duplicada em mais lugares), o custo de manter esse mapeamento manual explícito foi
considerado menor que o custo/risco de gerenciar uma license key. Essa decisão está
registrada aqui para não ser reavaliada sem esse contexto.
