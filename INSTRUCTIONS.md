# Bruinen – Developer Instructions

Bruinen is an ASP.NET Core web application for user authentication and account management. It uses PostgreSQL for storage, Caddy as a reverse proxy, and runs in a Docker-based development environment.

---

## Solution Structure

```
Bruinen.sln
├── src/
│   ├── Bruinen.Domain        – Domain entities (User, RequestCounter)
│   ├── Bruinen.Application   – Application services and repository abstractions
│   ├── Bruinen.Infra         – EF Core DbContext, migrations, repository implementations
│   └── Bruinen.Host          – ASP.NET Core MVC host (controllers, middleware, views)
└── tests/
    └── Bruinen.UnitTests     – xUnit unit tests
```

### Project responsibilities

| Project | Role |
|---|---|
| `Bruinen.Domain` | Plain C# domain models with no framework dependencies |
| `Bruinen.Application` | Business logic services (`LoginService`, `AccountService`, `RateLimitingService`) and repository interfaces |
| `Bruinen.Infra` | EF Core (`BruinenContext`), PostgreSQL configuration, migrations, and repository implementations |
| `Bruinen.Host` | MVC controllers, Razor views, middleware pipeline, and application entry point |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL and Caddy)
- Add the following entries to `/etc/hosts`:

```
127.0.0.1 auth.home.bruinen
127.0.0.1 app.home.bruinen
```

---

## Getting Started

### 1. Restore dependencies

```sh
make restore        # Restore NuGet packages
make tool-restore   # Install .NET local tools (EF Core CLI)
make libman-restore # Restore client-side libraries (Bootstrap, Font Awesome)
```

### 2. Start the development environment

```sh
make up   # Starts PostgreSQL (port 5437) and Caddy reverse proxy via Docker Compose
```

### 3. Apply database migrations

```sh
make migrate
```

### 4. Run the application

```sh
make run  # Runs src/Bruinen.Host on https://localhost:5001
```

The app is also accessible through the Caddy reverse proxy:

| URL | Description |
|---|---|
| `http://auth.home.bruinen` | Login / auth service (proxied to the Host on port 5001) |
| `http://app.home.bruinen` | Protected app (forward-auth enforced by Caddy via `/auth/verify`) |

---

## Common Development Commands

All commands are available via the `Makefile`. Run `make help` for a quick reference.

### Docker

| Command | Description |
|---|---|
| `make up` | Start containers in detached mode |
| `make down` | Stop and remove containers |
| `make restart` | Restart containers |
| `make logs` | Tail container logs |
| `make clean` | Stop containers and **delete volumes** (resets the database) |

### .NET

| Command | Description |
|---|---|
| `make restore` | Restore NuGet packages |
| `make build` | Build the entire solution |
| `make run` | Run `Bruinen.Host` |

### Database Migrations (EF Core)

| Command | Description |
|---|---|
| `make tool-restore` | Install EF Core CLI tools from `.config/dotnet-tools.json` |
| `make migrate` | Apply pending migrations to the local database |
| `make migration-add <Name>` | Scaffold a new migration (e.g. `make migration-add AddUserEmail`) |

Migrations are placed in `src/Bruinen.Infra/Migrations/` and use `BruinenContext` as the target context.

### Client-Side Libraries (LibMan)

| Command | Description |
|---|---|
| `make libman-restore` | Download configured libraries into `wwwroot/lib` |
| `make libman-clean` | Remove downloaded libraries |
| `make libman-update <lib>` | Update a single library (e.g. `make libman-update bootstrap`) |

LibMan is configured in `src/Bruinen.Host/libman.json`.

---

## Configuration

### `appsettings.json` (base)

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string (empty by default; set per environment) |
| `Authentication:CookieDomain` | Cookie domain shared across subdomains (`.home.bruinen`) |
| `Authentication:LoginUrl` | Redirect target for unauthenticated requests |
| `HeaderAllowList:AllowedHeaders` | Headers permitted to pass through the allow-list middleware |
| `Serilog` | Structured logging configuration (console + PostgreSQL sinks) |

### `appsettings.Development.json`

Overrides `ConnectionStrings:DefaultConnection` for the local Docker database:

```
Host=localhost;Port=5437;Database=bruinen;Username=postgres;Password=postgres
```

---

## Authentication

- Cookie-based authentication using the `CookieAuth` scheme.
- Cookies are scoped to `.home.bruinen` and are HTTP-only.
- Sessions persist for **7 days** with sliding expiration, or **30 days** if *Remember Me* is selected on login.
- The Caddy reverse proxy performs **forward authentication**: all requests to `app.home.bruinen` are checked against `/auth/verify` on the Host before being forwarded.

---

## Middleware Pipeline

| Middleware | Purpose |
|---|---|
| `HeaderAllowListMiddleware` | Strips request headers not present in the configured allow list |
| `RequestIdMiddleware` | Attaches a unique `X-Request-Id` to every request |
| `SerilogRequestLogging` | Structured HTTP access logging |
| `UseAuthentication` / `UseAuthorization` | Cookie auth and route protection |

---

## Rate Limiting

Login attempts are rate-limited via the `[RequestRateLimit]` action filter attribute.

- Default: **5 requests** per IP/action combination within a **30-second** window.
- Exceeded requests receive HTTP `429 Too Many Requests` with a `Retry-After` header.
- Counters are stored in the `RequestCounters` table via `IRequestCounterRepository`.

---

## Testing

Unit tests live in `tests/Bruinen.UnitTests` and use **xUnit**.

```sh
dotnet test
```

Test coverage areas:

- `ApplicationServices/` – service logic (login, account, rate limiting)
- `Domain/` – domain model behaviour
- `Middleware/` – middleware unit tests

---

## Adding a New Feature (Checklist)

1. Add or update domain models in `Bruinen.Domain`.
2. Define any new repository interfaces in `Bruinen.Application/Abstractions/`.
3. Implement repository changes in `Bruinen.Infra` and add EF Core configuration under `Configurations/`.
4. Scaffold a migration: `make migration-add <Name>`.
5. Add application service logic in `Bruinen.Application/Services/`.
6. Register new services in `Bruinen.Application/Setup.cs` or `Bruinen.Infra/Setup.cs`.
7. Add controllers, models, and views in `Bruinen.Host`.
8. Write unit tests in `Bruinen.UnitTests`.

