.PHONY: help up down restart logs clean build restore run migrate migration-add tool-restore libman-restore libman-clean libman-update libman-install

.DEFAULT_GOAL := help

help:
	@echo "Available commands:"
	@echo ""
	@echo "Docker commands:"
	@echo "  make up              - Start Docker containers in detached mode"
	@echo "  make down            - Stop and remove Docker containers"
	@echo "  make restart         - Restart Docker containers"
	@echo "  make logs            - View and follow container logs"
	@echo "  make clean           - Stop containers and remove volumes"
	@echo ""
	@echo ".NET commands:"
	@echo "  make restore         - Restore .NET dependencies"
	@echo "  make build           - Build the .NET solution"
	@echo "  make run             - Run the Host application"
	@echo ""
	@echo "Database migration commands:"
	@echo "  make tool-restore    - Install EF Core CLI tools"
	@echo "  make migrate         - Apply migrations to database"
	@echo "  make migration-add <name> - Create new migration (e.g., make migration-add AddUserEmail)"
	@echo ""
	@echo "Client-side library commands:"
	@echo "  make libman-restore  - Restore client-side libraries (Bootstrap, Font Awesome)"
	@echo "  make libman-clean    - Clean client-side libraries from wwwroot/lib"
	@echo "  make libman-update <lib> - Update a library (e.g., make libman-update bootstrap)"

up:
	docker compose up -d

down:
	docker compose down

restart:
	docker compose restart

logs:
	docker compose logs -f

clean:
	docker compose down -v

build:
	dotnet build

restore:
	dotnet restore

run:
	dotnet run --project src/Bruinen.Host/Bruinen.Host.csproj

tool-restore:
	dotnet tool restore

migrate:
	dotnet ef database update \
		--project ./src/Bruinen.Infra/Bruinen.Infra.csproj \
		--startup-project ./src/Bruinen.Host/Bruinen.Host.csproj \
		--connection "Host=localhost;Port=5437;Database=bruinen;Username=postgres;Password=postgres"

migration-add:
	@if [ -z "$(filter-out $@,$(MAKECMDGOALS))" ]; then \
		echo "Error: Migration name is required. Usage: make migration-add MigrationName"; \
		exit 1; \
	fi
	dotnet ef migrations add $(filter-out $@,$(MAKECMDGOALS)) \
		--context BruinenContext \
		--project ./src/Bruinen.Infra/Bruinen.Infra.csproj \
		--startup-project ./src/Bruinen.Host/Bruinen.Host.csproj

libman-restore:
	cd src/Bruinen.Host && dotnet libman restore

libman-clean:
	cd src/Bruinen.Host && dotnet libman clean

libman-update:
	@if [ -z "$(filter-out $@,$(MAKECMDGOALS))" ]; then \
		echo "Error: Library name is required. Usage: make libman-update bootstrap"; \
		exit 1; \
	fi
	cd src/Bruinen.Host && dotnet libman update $(filter-out $@,$(MAKECMDGOALS))

%:
	@:

