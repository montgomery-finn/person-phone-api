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
- **`PersonPhone.Infrastructure`** — duas implementações de repositório, alternáveis por
  configuração (veja a seção "Persistência"): in-memory (`InMemoryRepository<T>` genérico
  + `InMemoryPersonRepository`, `InMemoryPhoneRepository`, baseada em
  `ConcurrentDictionary`) e EF Core/SQL Server (`PersonPhoneDbContext`,
  `EfPersonRepository`, `EfPhoneRepository`, em `Persistence/`). Depende apenas de
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

A API suporta duas implementações de repositório, alternáveis por configuração — sem
precisar recompilar nem alterar código:

| Provider | Chave `Persistence:Provider` | Onde os dados vivem |
|---|---|---|
| **In-Memory** (padrão) | `InMemory` | `ConcurrentDictionary` estático em `InMemoryRepository<T>`. Dados existem só durante a execução do processo e são perdidos a cada restart. |
| **SQL Server (EF Core)** | `SqlServer` | Banco `PersonPhoneDb` no container `sqlserver` do `docker-compose.yml`, via `PersonPhoneDbContext`/`EfPersonRepository`/`EfPhoneRepository`. |

O provider é lido de `Persistence:Provider` (em `appsettings.json`/
`appsettings.{Environment}.json`) e decidido em `Program.cs`. `appsettings.json` (base)
usa `InMemory` como default seguro — comportamento inalterado para quem não configurar
nada. `appsettings.Development.json` já vem configurado com `SqlServer` e a connection
string apontando para o container local — como `dotnet run` usa o ambiente `Development`
por padrão (via `launchSettings.json`), é necessário subir o `docker compose` antes de
rodar a API localmente (veja a seção seguinte).

Para voltar ao in-memory em desenvolvimento sem editar arquivos, sobrescreva a chave via
variável de ambiente:

```
Persistence__Provider=InMemory dotnet run --project src/PersonPhone.Api
```

## Banco de dados (SQL Server via Docker)

1. Subir o container (a partir da raiz do repositório):

   ```
   docker compose up -d
   ```

   Isso inicia um SQL Server 2022 (edição Developer, gratuita para dev/test) na porta
   `1433`, usuário `sa` e a senha definida em `docker-compose.yml` — **senha de uso local
   apenas, não usar em produção**. Os dados persistem entre restarts do container graças
   ao volume nomeado `sqlserver_data` (só são perdidos com `docker compose down -v`).

2. Instalar a ferramenta `dotnet-ef` (uma vez por checkout — a versão já está pinada em
   `.config/dotnet-tools.json`):

   ```
   dotnet tool restore
   ```

3. Aplicar as migrations (cria o banco `PersonPhoneDb` e as tabelas `People`/`Phones`):

   ```
   ASPNETCORE_ENVIRONMENT=Development dotnet tool run dotnet-ef database update --project src/PersonPhone.Infrastructure --startup-project src/PersonPhone.Api
   ```

   > A variável `ASPNETCORE_ENVIRONMENT=Development` é necessária porque os comandos
   > `dotnet ef` não passam pelo `launchSettings.json` — sem ela, a ferramenta lê
   > `appsettings.json` base (provider `InMemory`) e não encontra nenhum `DbContext`
   > registrado.

4. Rodar a API normalmente (`dotnet run --project src/PersonPhone.Api`) — como o profile
   de desenvolvimento já usa `Persistence:Provider = SqlServer`, ela conversa direto com
   o container.

Migrations são aplicadas **manualmente** (não há `Database.Migrate()` automático no
startup da aplicação) — assim o schema do banco só muda quando alguém explicitamente
rodar o comando acima.

## Rodando tudo via Docker Compose (api + SQL Server)

Além do fluxo de dev acima (`dotnet run` local + `sqlserver` via compose), o
`docker-compose.yml` também tem um serviço `api`, que builda a imagem de produção da API
a partir do `Dockerfile` na raiz do repositório (multi-stage: `dotnet publish` em
`Release` numa imagem `sdk:10.0`, copiado para uma imagem final `aspnet:10.0`).

```
docker compose up -d --build
```

Isso sobe os dois containers:

- `sqlserver` — igual ao fluxo de dev.
- `api` — só inicia depois que o `sqlserver` responde ao healthcheck
  (`depends_on: condition: service_healthy`), já configurada via variáveis de ambiente
  no `docker-compose.yml` (`Persistence__Provider=SqlServer` e
  `ConnectionStrings__DefaultConnection` apontando para `Server=sqlserver,1433` — o nome
  do serviço, resolvido pela rede interna do compose). Fica disponível em
  `http://localhost:8080`.

**As migrations continuam manuais**, mesmo nesse fluxo: o container `api` não roda
`dotnet ef database update` sozinho. Antes de bater nos endpoints que tocam o banco
(`/person`, `/phone`), aplique as migrations com o mesmo comando da seção anterior,
rodando do host contra `localhost:1433` (a porta do `sqlserver` continua publicada
normalmente):

```
dotnet tool restore
ASPNETCORE_ENVIRONMENT=Development dotnet tool run dotnet-ef database update --project src/PersonPhone.Infrastructure --startup-project src/PersonPhone.Api
```

Sem esse passo, a api sobe normalmente, mas qualquer chamada que toque o banco retorna
erro (tabela inexistente).

### Gerando novas migrations

Sempre que o modelo (`PersonPhoneDbContext` ou `Persistence/Configurations/*`) mudar:

```
ASPNETCORE_ENVIRONMENT=Development dotnet tool run dotnet-ef migrations add <NomeDaMigration> --project src/PersonPhone.Infrastructure --startup-project src/PersonPhone.Api --output-dir Persistence/Migrations
```

e depois reaplicar com o mesmo comando `dotnet ef database update` do passo 3.

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

  Por padrão isso roda em ambiente `Development`, que já está configurado para usar
  SQL Server via EF Core (`Persistence:Provider = SqlServer` em
  `appsettings.Development.json`) — é necessário subir o container e aplicar as
  migrations antes (veja "Banco de dados (SQL Server via Docker)"), ou sobrescrever
  `Persistence:Provider` para `InMemory` caso queira rodar sem Docker.

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

### Construtor vazio nas entidades (`Person()`, `Phone()`)

`Person` e `Phone` têm um construtor público sem parâmetros além do construtor
principal, que existe só para satisfazer o EF Core — ele precisa de um construtor
(mesmo que privado) para materializar entidades a partir do banco, e o mapeamento via
`Persistence/Configurations/*` usado aqui exige que esse construtor seja acessível.

Isso é uma concessão ao framework, não o ideal: em um cenário sem essa restrição de
escopo/tempo, as entidades de domínio não deveriam expor nenhum construtor que permita
criar um objeto em estado inválido/vazio — toda instância deveria nascer já válida, via
o construtor principal. A forma correta de resolver isso seria persistir, no EF Core,
classes de modelo (persistence models) separadas das entidades de domínio, com e mapear 
explicitamente entidade ↔ modelo na leitura e escrita. Isso mantém
o `Domain` genuinamente independente de framework, sem esse acoplamento ao
requisito técnico do EF Core.

Essa separação não foi feita aqui por decisão consciente de escopo e tempo — o
construtor vazio foi o caminho mais simples para o tamanho atual do projeto.

### `appsettings.Development.json` commitado com credencial

`appsettings.Development.json` está commitado no repositório já com a connection
string do SQL Server, incluindo usuário e senha (`sa` / `YourStr0ng!Passw0rd` — a mesma
senha de uso local definida em `docker-compose.yml`).

Isso foi feito por conveniência, para que o projeto rode localmente sem nenhuma
configuração extra além de subir o `docker compose`. Não é uma boa prática — segredos
não deveriam ser versionados em controle de código, mesmo quando o valor em si é
inofensivo (senha de container local, sem exposição externa). Em um ambiente de
produção isso não deve se repetir: a connection string (e qualquer outro segredo)
deve vir de uma fonte externa ao repositório — variável de ambiente, secret manager
(Azure Key Vault, AWS Secrets Manager etc.) ou `dotnet user-secrets` —, nunca de um
arquivo `appsettings*.json` versionado.
