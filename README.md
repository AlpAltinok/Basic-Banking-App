# BankaApp — Dijital Cüzdan ve Finansal İşlem API

VeriPark / bankacılık yazılım stack'ine (.NET 8, ASP.NET Core Web API, EF Core, SQL Server, JWT, Unit Test) odaklı öğrenme ve portföy projesi.

## Mimari

```
BankaApp.sln
├── src/
│   ├── BankaApp.Api            → HTTP uç noktaları, middleware, Swagger
│   ├── BankaApp.Application    → İş kuralları, DTO, servis arayüzleri
│   ├── BankaApp.Domain         → Entity ve enum'lar (saf C#, framework yok)
│   └── BankaApp.Infrastructure → EF Core, repository, JWT, BCrypt
└── tests/
    └── BankaApp.UnitTests
```

**Neden katmanlı?** Bankacılık ekiplerinde API, iş kuralı ve veri erişimi ayrılır. Domain framework'ten bağımsız kalır; test ve bakım kolaylaşır.

## Şu an ne var?

- [x] Solution + katmanlı yapı
- [x] Domain: `User`, `Wallet`, `Transaction`
- [x] EF Core Code-First + Fluent API
- [x] Auth: Register / Login + JWT
- [x] Global exception middleware
- [x] Wallet: bakiye, deposit, withdraw, hareket listesi
- [x] Para transferi (DB transaction + idempotency)
- [x] Unit testler (Auth + Wallet + Transfer)
- [x] EF Core migrations + kalıcı SQLite DB (SQL Server hazır)
- [x] CI: GitHub Actions + Azure Pipelines YAML

## Çalıştırma

```bash
dotnet restore
dotnet run --project src/BankaApp.Api
```

Swagger: `http://localhost:5088/swagger`

Varsayılan DB: **SQLite** (`BankaApp.db`) — restart’ta veri kalır, migration uygulanır.

```json
"Database": { "Provider": "Sqlite" }
```

SQL Server (LocalDB / Express kuruluysa):

```json
"Database": { "Provider": "SqlServer" }
```

Yeni migration (şema değişince):

```bash
dotnet ef migrations add <Ad> --project src/BankaApp.Infrastructure --startup-project src/BankaApp.Api --output-dir Persistence/Migrations
```

## CI / CD

| Dosya | Ne için? |
|---|---|
| `.github/workflows/ci.yml` | GitHub’a push/PR → otomatik restore + build + test |
| `azure-pipelines.yml` | Azure DevOps (VeriPark stack’i) için aynı pipeline |

Yerelde CI’nin yaptığını çalıştırmak:

```bash
dotnet restore BankaApp.sln
dotnet build BankaApp.sln -c Release
dotnet test BankaApp.sln -c Release --no-build
```

## Örnek istekler

**Kayıt**

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "eren@example.com",
  "fullName": "Eren",
  "password": "Sifre123!"
}
```

**Giriş**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "eren@example.com",
  "password": "Sifre123!"
}
```

**Cüzdan (Authorization: Bearer {token} gerekir)**

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
  "toEmail": "veli@banka.app",
  "amount": 150,
  "description": "Borç ödemesi",
  "idempotencyKey": "unique-key-123"
}
```
