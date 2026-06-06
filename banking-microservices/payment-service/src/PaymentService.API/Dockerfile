FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/PaymentService.API/PaymentService.API.csproj",           "src/PaymentService.API/"]
COPY ["src/PaymentService.Application/PaymentService.Application.csproj", "src/PaymentService.Application/"]
COPY ["src/PaymentService.Domain/PaymentService.Domain.csproj",     "src/PaymentService.Domain/"]
COPY ["src/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "src/PaymentService.Infrastructure/"]

RUN dotnet restore "src/PaymentService.API/PaymentService.API.csproj"

COPY . .
RUN dotnet build "src/PaymentService.API/PaymentService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/PaymentService.API/PaymentService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
