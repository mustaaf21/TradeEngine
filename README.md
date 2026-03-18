# TradeEngine API

A production-style backend trading engine built with **.NET 8, ASP.NET Core, SQL Server, and Entity Framework Core**, following **Clean Architecture** principles.

Supports the full trading lifecycle — account management, order placement, price-time priority matching, T+1 settlement, and portfolio analytics — secured with JWT authentication and rate limiting.

> Built as a personal project to demonstrate financial systems design patterns outside of insurance billing work.

---

## Architecture

The solution is split into 4 independent projects with strict inward dependency flow:

```
TradeEngine.API              → Presentation layer (Controllers, Middleware, Program.cs)
TradeEngine.Application      → Contracts layer (Interfaces, DTOs)
TradeEngine.Infrastructure   → Implementation layer (Services, DbContext, Migrations)
TradeEngine.Domain           → Core layer (Entities, Enums, Exceptions)
```

**Dependency rule:** each layer only depends on the layer directly below it. The Domain layer has zero external dependencies — pure business logic.

```
API  →  Application  →  Domain
              ↑
       Infrastructure
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8, ASP.NET Core Web API |
| Database | SQL Server, Entity Framework Core 8 |
| Authentication | JWT Bearer + Refresh Token Rotation |
| Password Security | PBKDF2-SHA256, 100K iterations |
| Background Services | IHostedService (T+1 settlement worker) |
| Rate Limiting | ASP.NET Core built-in rate limiter |
| API Docs | Swagger / OpenAPI |
| Concurrency | Optimistic concurrency (RowVersion) |

---

## Features

### Trading
- Buy and sell limit orders with fund freezing on placement
- Market orders — execute immediately at best available price
- Stop Loss and Stop Limit orders — trigger on price threshold
- Price-time priority order matching engine
- Real-time order book with bid/ask aggregation
- Order cancellation with automatic fund release

### Financial Operations
- Double-entry ledger — every financial event writes a debit/credit entry
- Position tracking with weighted average cost basis
- T+1 settlement via background worker running hourly
- Full audit trail across all transactions

### Analytics
- Realized P&L from closed positions
- Unrealized P&L with mark-to-market valuation
- Portfolio summary — total value, cash balance, position weights
- Ledger statements with running balance and optional date filtering

### Security
- JWT access tokens (15 min) with refresh token rotation
- Refresh tokens revoked on use and on logout
- PBKDF2-SHA256 password hashing with random salt and timing-safe comparison
- Global rate limit: 100 requests/min
- Trading endpoint rate limit: 30 requests/min
- Idempotency keys on orders to prevent duplicate submissions

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Setup

1. **Clone the repository**
```bash
git clone https://github.com/mustaaf21/TradeEngine.git
cd TradeEngine
```

2. **Configure User Secrets** (never put secrets in appsettings.json)

Right-click `TradeEngine.API` in Visual Studio → Manage User Secrets, then add:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TradeEngineDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long",
    "Issuer": "TradeEngine",
    "Audience": "TradeEngineUsers",
    "AccessTokenExpirationMinutes": "15",
    "RefreshTokenExpirationDays": "7"
  }
}
```

3. **Run migrations**

In Visual Studio Package Manager Console:
```powershell
Update-Database -Project TradeEngine.Infrastructure -StartupProject TradeEngine.API
```

4. **Run the application**
```bash
dotnet run --project TradeEngine.API/WebApplication1
```

5. **Open Swagger UI**
```
https://localhost:7130/swagger
```

---

## API Reference

### Authentication
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | No | Login, returns access + refresh token |
| POST | `/api/auth/refresh` | No | Rotate refresh token |
| POST | `/api/auth/logout` | Yes | Revoke all refresh tokens |

### Accounts
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/accounts` | No | Create account |
| GET | `/api/accounts/{id}` | Yes | Get account details and balance |
| POST | `/api/accounts/{id}/deposit` | Yes | Deposit funds |

### Orders
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/orders/buy` | Yes | Place buy order |
| POST | `/api/orders/sell` | Yes | Place sell order |
| POST | `/api/orders/{id}/execute` | Yes | Execute order directly |
| POST | `/api/orders/{id}/cancel` | Yes | Cancel order, release frozen funds |
| GET | `/api/orders/{id}` | Yes | Get order details |
| GET | `/api/orders/account/{id}` | Yes | Get all orders for account |
| GET | `/api/orders/account/{id}/trades` | Yes | Get trade history |
| GET | `/api/orders/account/{id}/positions` | Yes | Get current positions |

### Market
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/market/orderbook/{symbol}` | No | Get live order book |
| GET | `/api/market/price/{symbol}` | No | Get current price |
| POST | `/api/market/match/{orderId}` | Yes | Match order against book |
| POST | `/api/market/price/{symbol}` | Yes | Set price (demo use) |

### Analytics
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/analytics/account/{id}/pnl` | Yes | Realized + unrealized P&L |
| GET | `/api/analytics/account/{id}/portfolio` | Yes | Portfolio summary with weights |
| GET | `/api/analytics/account/{id}/ledger` | Yes | Ledger statement (supports ?from=&to=) |

### Settlement
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/settlement/trade/{tradeId}` | Yes | Settle a specific trade |
| POST | `/api/settlement/process-pending` | Yes | Batch settle all eligible trades |
| GET | `/api/settlement/pending` | Yes | List unsettled trades |

---

## How a Trade Works

```
1. Create account          POST /api/accounts
2. Deposit funds           POST /api/accounts/{id}/deposit
3. Login                   POST /api/auth/login  →  copy accessToken
4. Authorize               Swagger: Authorize → Bearer <token>
5. Place buy order         POST /api/orders/buy  →  funds frozen
6. Place sell order        POST /api/orders/sell (second account)
7. Match orders            POST /api/market/match/{orderId}
8. Check analytics         GET  /api/analytics/account/{id}/portfolio
9. Settlement (auto)       Background worker runs hourly after T+1
```

---

## Financial Design

### Fund Freezing
When a buy order is placed, the required funds are frozen immediately:
```
Available Balance = Total Balance - Frozen Balance
```
On execution, frozen funds are unfrozen and deducted. On cancellation, frozen funds are released.

### Order State Machine
```
Created → PartiallyFilled → Executed → Settled
    └──────── Cancelled
```

### Double-Entry Ledger
Every financial movement creates a ledger entry:

| Event | Debit | Credit |
|---|---|---|
| Deposit | 0 | amount |
| Buy trade | trade value | 0 |
| Sell trade | 0 | trade value |
| Settlement | 0 | 0 (record only) |

### Optimistic Concurrency
Account, Order, and Position entities use SQL Server `rowversion` columns. EF Core includes the version in every UPDATE — if another transaction modified the record first, a `DbUpdateConcurrencyException` is thrown and returns HTTP 409.

---

## Project Structure

```
TradeEngine.Domain/
├── Entities/          Account, Order, Trade, Position, LedgerEntry, RefreshToken
├── Enums/             OrderSide, OrderType, OrderStatus, TradeStatus
└── Exceptions/        AccountNotFoundException, InsufficientFundsException, ...

TradeEngine.Application/
├── Interfaces/        IAccountService, IOrderService, IMatchingEngine, ...
└── DTOs/              CreateAccountRequest, PlaceOrderRequest, LoginRequest

TradeEngine.Infrastructure/
├── Persistence/       TradeEngineDbContext
├── Service/           AccountService, OrderService, MatchingEngine, AuthService,
│                      SettlementService, AnalyticsService, PriceService,
│                      SettlementBackgroundService
└── Migrations/

TradeEngine.API/
├── Controllers/       AccountsController, OrdersController, MarketController,
│                      AuthController, AnalyticsController, SettlementController
├── Middlewares/       ExceptionMiddleware
└── Program.cs
```

---

## License

MIT — free to use for learning and reference.
