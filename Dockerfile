# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BankaApp.sln ./
COPY src/BankaApp.Api/BankaApp.Api.csproj src/BankaApp.Api/
COPY src/BankaApp.Application/BankaApp.Application.csproj src/BankaApp.Application/
COPY src/BankaApp.Domain/BankaApp.Domain.csproj src/BankaApp.Domain/
COPY src/BankaApp.Infrastructure/BankaApp.Infrastructure.csproj src/BankaApp.Infrastructure/
COPY tests/BankaApp.UnitTests/BankaApp.UnitTests.csproj tests/BankaApp.UnitTests/
RUN dotnet restore BankaApp.sln
COPY . .
RUN dotnet publish src/BankaApp.Api/BankaApp.Api.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5088
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5088
ENTRYPOINT ["dotnet", "BankaApp.Api.dll"]
