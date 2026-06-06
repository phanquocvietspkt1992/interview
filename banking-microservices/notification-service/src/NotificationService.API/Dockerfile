FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/NotificationService.API/NotificationService.API.csproj",           "src/NotificationService.API/"]
COPY ["src/NotificationService.Application/NotificationService.Application.csproj", "src/NotificationService.Application/"]
COPY ["src/NotificationService.Domain/NotificationService.Domain.csproj",     "src/NotificationService.Domain/"]
COPY ["src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/NotificationService.Infrastructure/"]

RUN dotnet restore "src/NotificationService.API/NotificationService.API.csproj"

COPY . .
RUN dotnet build "src/NotificationService.API/NotificationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/NotificationService.API/NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
