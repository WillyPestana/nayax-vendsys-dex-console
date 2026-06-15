FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /workspace

COPY global.json ./
COPY src/backend ./src/backend
COPY database ./database

RUN dotnet restore src/backend/NayaxVendSys.sln
RUN dotnet publish src/backend/src/NayaxVendSys.Api/NayaxVendSys.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./
COPY database ./database

EXPOSE 8080
ENTRYPOINT ["dotnet", "NayaxVendSys.Api.dll"]
