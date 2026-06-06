FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/LoanService.API/LoanService.API.csproj",           "src/LoanService.API/"]
COPY ["src/LoanService.Application/LoanService.Application.csproj", "src/LoanService.Application/"]
COPY ["src/LoanService.Domain/LoanService.Domain.csproj",     "src/LoanService.Domain/"]
COPY ["src/LoanService.Infrastructure/LoanService.Infrastructure.csproj", "src/LoanService.Infrastructure/"]

RUN dotnet restore "src/LoanService.API/LoanService.API.csproj"

COPY . .
RUN dotnet build "src/LoanService.API/LoanService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/LoanService.API/LoanService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoanService.API.dll"]
