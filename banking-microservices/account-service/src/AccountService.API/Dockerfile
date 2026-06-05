FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/AccountService.API/AccountService.API.csproj",           "src/AccountService.API/"]
COPY ["src/AccountService.Application/AccountService.Application.csproj", "src/AccountService.Application/"]
COPY ["src/AccountService.Domain/AccountService.Domain.csproj",     "src/AccountService.Domain/"]
COPY ["src/AccountService.Infrastructure/AccountService.Infrastructure.csproj", "src/AccountService.Infrastructure/"]

RUN dotnet restore "src/AccountService.API/AccountService.API.csproj"

COPY . .
RUN dotnet build "src/AccountService.API/AccountService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/AccountService.API/AccountService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.API.dll"]
