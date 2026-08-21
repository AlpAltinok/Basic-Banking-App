# GoldArrow Bank — Digital Wallet & Transfer API

A layered .NET 8 backend that models everyday fintech flows: authentication, wallet balances, ledger entries, and peer-to-peer transfers with transactional consistency.

Product UI is branded **GoldArrow Bank**. The solution folders remain `BankaApp.*` for the codebase.

Built as a portfolio project around common enterprise banking/fintech patterns — not tied to any specific employer.

## Stack

- ASP.NET Core Web API (.NET 8)
- Blazor WebAssembly UI (.NET 9)
- Clean / layered architecture (Api · Application · Domain · Infrastructure)
- EF Core (Code-First) with migrations
- SQLite by default; SQL Server ready via config
- JWT authentication + BCrypt password hashing
- Unit & integration tests (xUnit, Moq, FluentAssertions, WebApplicationFactory)
- CI: GitHub Actions + Azure Pipelines YAML

## Architecture

```
BankaApp.sln
├── src/
│   ├── BankaApp.Api            → HTTP endpoints, middleware, Swagger
│   ├── BankaApp.Application    → Use cases, DTOs, service interfaces
│   ├── BankaApp.Domain         → Entities & enums (framework-free)
│   ├── BankaApp.Infrastructure → EF Core, repositories, JWT, BCrypt
│   └── BankaApp.Web            → Blazor WASM client (login, wallet, transfer)
└── tests/
    ├── BankaApp.UnitTests
    └── BankaApp.IntegrationTests
```

**Why layers?** In financial systems, HTTP, business rules, and data access are separated so rules stay testable and the domain stays independent of frameworks.

## Features

- [x] Layered solution structure
- [x] Domain model: `User`, `Wallet`, `Transaction`
- [x] EF Core Code-First + Fluent API
- [x] Auth: register / login + JWT
- [x] Global exception middleware
- [x] Wallet: balance, deposit, withdraw, transaction history
- [x] Transfers with DB transactions + idempotency keys
- [x] Unit tests (Auth, Wallet, Transfer)
- [x] Integration tests (HTTP E2E: auth → deposit → transfer + idempotency)
- [x] Optimistic concurrency on wallet balance updates
- [x] EF Core migrations + persistent SQLite (SQL Server option)
- [x] CI pipelines (GitHub Actions + Azure Pipelines)
- [x] Docker image & compose
- [x] Blazor WebAssembly frontend (auth, wallet, transfer)

## Run locally

**API**

```bash
dotnet run --project src/BankaApp.Api --urls http://localhost:5088
```

Swagger: [http://localhost:5088/swagger](http://localhost:5088/swagger)

**Web UI** (second terminal)

```bash
dotnet run --project src/BankaApp.Web --urls http://localhost:5274
```

Open [http://localhost:5274](http://localhost:5274) — register/sign in, then deposit and transfer.

API base URL for the client: `src/BankaApp.Web/wwwroot/appsettings.json` (`ApiBaseUrl`).

### Database

Default provider is **SQLite** (`BankaApp.db` under the API project). Data survives restarts; migrations run on startup.

```json
"Database": { "Provider": "Sqlite" }
```

SQL Server (LocalDB / Express / remote):

```json
"Database": { "Provider": "SqlServer" }
```

New migration after model changes:

```bash
dotnet ef migrations add <Name> \
  --project src/BankaApp.Infrastructure \
  --startup-project src/BankaApp.Api \
  --output-dir Persistence/Migrations
```

### Docker

```bash
docker compose up --build
```

API: [http://localhost:5088/swagger](http://localhost:5088/swagger)

## CI

| File | Purpose |
|---|---|
| `.github/workflows/ci.yml` | On push/PR: restore → build → test |
| `azure-pipelines.yml` | Same pipeline shape for Azure DevOps |

Run the same checks locally:

```bash
dotnet restore BankaApp.sln
dotnet build BankaApp.sln -c Release
dotnet test BankaApp.sln -c Release --no-build
```

## Example requests

**Register**

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "alice@example.com",
  "fullName": "Alice",
  "password": "Password123!"
}
```

**Login**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "alice@example.com",
  "password": "Password123!"
}
```

In Swagger: **Authorize** → paste `Bearer {accessToken}` (include the word `Bearer`).

**Wallet** (requires JWT)

```http
GET  /api/wallet
POST /api/wallet/deposit
POST /api/wallet/withdraw
GET  /api/wallet/transactions
```

**Transfer**

```http
POST /api/transfers
Authorization: Bearer {token}
Content-Type: application/json

{
  "toEmail": "bob@example.com",
  "amount": 150,
  "description": "Rent share",
  "idempotencyKey": "unique-key-123"
}
```

## Design notes (interview-friendly)

- **Money as `decimal`** — never `float`/`double` for currency
- **Ledger rows** — every deposit, withdrawal, and transfer is recorded, not only balance updates
- **Atomic transfers** — debit + credit + ledger write succeed or roll back together
- **Idempotency** — optional client key prevents double-charging on retries
- **Optimistic concurrency** — `Wallet.Version` blocks lost updates on concurrent withdrawals (HTTP 409)
- **Passwords hashed** — BCrypt; plain text is never stored
