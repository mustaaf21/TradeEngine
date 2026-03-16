# 🏦 TradeEngine

A **production-grade backend trading engine** built with **.NET 8, ASP.NET Core Web API, SQL Server, and Entity Framework Core**, following **Clean Architecture** and **Domain-Driven Design (DDD)** principles.

## 🎯 Overview

TradeEngine simulates a financial trading platform with complete support for:

- **Account Management** - Create accounts, deposits, balance tracking
- **Order Management** - Buy/Sell orders, Market/Limit/Stop orders
- **Trade Execution** - Matching engine with order book
- **Position Tracking** - Real-time portfolio positions with average cost
- **Ledger Accounting** - Double-entry bookkeeping for financial integrity
- **Settlement** - T+1 settlement simulation
- **Analytics** - PnL calculation, portfolio valuation

## 🏗️ Architecture

```
TradeEngine/
├── TradeEngine.API/              # Presentation Layer (Controllers, Middleware)
├── TradeEngine.Application/      # Application Layer (DTOs, Interfaces)
├── TradeEngine.Domain/           # Domain Layer (Entities, Enums, Exceptions)
└── TradeEngine.Infrastructure/   # Infrastructure Layer (EF Core, Services)
```

### Dependency Flow
```
API → Application → Domain
         ↓
   Infrastructure
```

**Domain layer has zero dependencies** - pure business logic.

## ✨ Features

### Core Trading
- ✅ **Buy & Sell Orders** - Place limit orders with price
- ✅ **Market Orders** - Execute immediately at best price
- ✅ **Stop Loss Orders** - Trigger when price reaches threshold
- ✅ **Order Cancellation** - Cancel open orders, release frozen funds
- ✅ **Order Matching Engine** - Price-time priority matching
- ✅ **Order Book** - Real-time bid/ask aggregation

### Financial Operations
- ✅ **Fund Freezing** - Lock funds when placing buy orders
- ✅ **Position Management** - Track holdings with average cost basis
- ✅ **Ledger Entries** - Complete audit trail of all transactions
- ✅ **T+1 Settlement** - Simulated settlement cycle

### Analytics
- ✅ **Realized P&L** - Profit/loss from closed positions
- ✅ **Unrealized P&L** - Mark-to-market valuation
- ✅ **Portfolio Summary** - Total value, cash, positions
- ✅ **Ledger Statements** - Transaction history with running balance

### Security
- ✅ **JWT Authentication** - Secure token-based auth
- ✅ **Password Hashing** - PBKDF2 with SHA256
- ✅ **Rate Limiting** - Prevent API abuse
- ✅ **Optimistic Concurrency** - RowVersion for race conditions

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | .NET 8, ASP.NET Core |
| Database | SQL Server, EF Core 8 |
| Authentication | JWT Bearer Tokens |
| Documentation | Swagger/OpenAPI |
| Architecture | Clean Architecture, DDD |

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)

### Setup

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/TradeEngine.git
cd TradeEngine
```

2. **Update connection string** in `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TradeEngineDb;Trusted_Connection=True;"
  }
}
```

3. **Run migrations**
```bash
cd TradeEngine.API/WebApplication1
dotnet ef database update
```

4. **Run the application**
```bash
dotnet run
```

5. **Open Swagger UI** at `https://localhost:5001`

## 📡 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login and get JWT token |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Revoke refresh tokens |

### Accounts
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/accounts` | Create new account |
| GET | `/api/accounts/{id}` | Get account details |
| POST | `/api/accounts/{id}/deposit` | Deposit funds |

### Orders
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/orders/buy` | Place buy order |
| POST | `/api/orders/sell` | Place sell order |
| POST | `/api/orders/{id}/execute` | Execute order |
| POST | `/api/orders/{id}/cancel` | Cancel order |
| GET | `/api/orders/{id}` | Get order details |
| GET | `/api/orders/account/{accountId}` | Get account orders |
| GET | `/api/orders/account/{accountId}/trades` | Get trade history |
| GET | `/api/orders/account/{accountId}/positions` | Get positions |

### Market
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/market/orderbook/{symbol}` | Get order book |
| GET | `/api/market/price/{symbol}` | Get current price |
| POST | `/api/market/match/{orderId}` | Match order against book |

### Analytics
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/analytics/account/{id}/pnl` | Get P&L report |
| GET | `/api/analytics/account/{id}/portfolio` | Get portfolio summary |
| GET | `/api/analytics/account/{id}/ledger` | Get ledger statement |

### Settlement
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/settlement/trade/{tradeId}` | Settle trade |
| POST | `/api/settlement/process-pending` | Process all pending |
| GET | `/api/settlement/pending` | Get pending settlements |

## 🧩 Design Patterns

| Pattern | Implementation |
|---------|----------------|
| **Clean Architecture** | Layered separation of concerns |
| **Domain-Driven Design** | Rich domain entities with behavior |
| **Repository Pattern** | EF Core DbContext as repository |
| **Unit of Work** | EF Core transaction management |
| **Service Layer** | Business workflow orchestration |
| **Dependency Injection** | ASP.NET Core DI container |

## 💰 Financial Integrity

### Ledger-Based Accounting
Every financial movement is recorded in the ledger with debit/credit entries.

### Fund Freezing
Buy orders freeze funds immediately, preventing overspending:
```
Available Balance = Balance - Frozen Balance
```

### Transaction Safety
All critical operations are wrapped in database transactions with rollback on failure.

### State Machine
Orders follow a strict state machine:
```
Created → PartiallyFilled → Executed → Settled
    ↓
 Cancelled
```

## 🔒 Security Features

### Password Security
- PBKDF2 with SHA256
- 100,000 iterations
- Random salt per password

### JWT Tokens
- Short-lived access tokens (15 min)
- Refresh token rotation
- Token revocation on logout

### Rate Limiting
- 100 requests/minute global
- 30 requests/minute for trading endpoints

## 📊 Domain Model

```
Account ──┬── Orders ──── Trades
          │
          ├── Positions
          │
          └── LedgerEntries
```

## 🎯 Interview Value

This project demonstrates:
- ✅ Clean Architecture implementation
- ✅ Domain-Driven Design principles
- ✅ Financial transaction safety
- ✅ Optimistic concurrency handling
- ✅ Real trading workflows
- ✅ Production-ready security
- ✅ Comprehensive API design

**Significantly stronger than typical CRUD projects.**

## 📝 License

MIT License - feel free to use for learning and interviews.

