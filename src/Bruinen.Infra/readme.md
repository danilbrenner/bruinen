## EF Core Migrations

### Create a migration for a specific DbContext

```bash
dotnet ef migrations add <MigratioName> \
         --context BruinenContext \
         --project ./src/Bruinen.Infra/Bruinen.Infra.csproj \
         --startup-project ./src/Bruinen.Host/Bruinen.Host.csproj
```

### Apply migrations to the database

```bash
dotnet ef database update --project ./src/Bruinen/Bruinen.csproj --connection "<CONNECTION_STRING>"
```

### Prerequisites

Ensure EF Core CLI tools are installed:

```bash
dotnet tool restore
```
