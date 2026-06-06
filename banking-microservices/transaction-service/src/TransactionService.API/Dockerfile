FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/TransactionService.API/TransactionService.API.csproj",           "src/TransactionService.API/"]
COPY ["src/TransactionService.Application/TransactionService.Application.csproj", "src/TransactionService.Application/"]
COPY ["src/TransactionService.Domain/TransactionService.Domain.csproj",     "src/TransactionService.Domain/"]
COPY ["src/TransactionService.Infrastructure/TransactionService.Infrastructure.csproj", "src/TransactionService.Infrastructure/"]

RUN dotnet restore "src/TransactionService.API/TransactionService.API.csproj"

COPY . .
RUN dotnet build "src/TransactionService.API/TransactionService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TransactionService.API/TransactionService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransactionService.API.dll"]
