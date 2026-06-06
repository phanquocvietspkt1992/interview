FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/IdentityService.API/IdentityService.API.csproj",           "src/IdentityService.API/"]
COPY ["src/IdentityService.Application/IdentityService.Application.csproj", "src/IdentityService.Application/"]
COPY ["src/IdentityService.Domain/IdentityService.Domain.csproj",     "src/IdentityService.Domain/"]
COPY ["src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "src/IdentityService.Infrastructure/"]

RUN dotnet restore "src/IdentityService.API/IdentityService.API.csproj"

COPY . .
RUN dotnet build "src/IdentityService.API/IdentityService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/IdentityService.API/IdentityService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.API.dll"]
