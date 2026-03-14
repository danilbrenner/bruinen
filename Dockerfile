FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .

COPY src/Bruinen.Domain/Bruinen.Domain.csproj src/Bruinen.Domain/
COPY src/Bruinen.Application/Bruinen.Application.csproj src/Bruinen.Application/
COPY src/Bruinen.Infra/Bruinen.Infra.csproj src/Bruinen.Infra/
COPY src/Bruinen.Host/Bruinen.Host.csproj src/Bruinen.Host/

RUN dotnet restore src/Bruinen.Host/Bruinen.Host.csproj

COPY src/ src/

RUN dotnet publish src/Bruinen.Host/Bruinen.Host.csproj \
    --no-restore \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Bruinen.Host.dll"]

