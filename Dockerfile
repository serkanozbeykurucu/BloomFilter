# Use the .NET 8 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy solution and project files
COPY BloomFilter.sln .
COPY src/BloomFilter.Business/BloomFilter.Business.csproj src/BloomFilter.Business/
COPY src/BloomFilter.DataAccess/BloomFilter.DataAccess.csproj src/BloomFilter.DataAccess/
COPY src/BloomFilter.Dto/BloomFilter.Dto.csproj src/BloomFilter.Dto/
COPY src/BloomFilter.Entity/BloomFilter.Entity.csproj src/BloomFilter.Entity/
COPY src/BloomFilter.HttpApi/BloomFilter.HttpApi.csproj src/BloomFilter.HttpApi/
COPY src/BloomFilter.HttpApi.Host/BloomFilter.HttpApi.Host.csproj src/BloomFilter.HttpApi.Host/
COPY src/BloomFilter.Shared/BloomFilter.Shared.csproj src/BloomFilter.Shared/
COPY test/BloomFilter.Tests/BloomFilter.Tests.csproj test/BloomFilter.Tests/

# Restore dependencies
RUN dotnet restore BloomFilter.sln

# Copy the rest of the application code
COPY . .

# Publish the application
RUN dotnet publish src/BloomFilter.HttpApi.Host/BloomFilter.HttpApi.Host.csproj -c Release -o /app/publish --no-restore

# Use the ASP.NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "BloomFilter.HttpApi.Host.dll"]
