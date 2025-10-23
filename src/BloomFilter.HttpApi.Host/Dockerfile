FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BloomFilter.HttpApi.Host/BloomFilter.HttpApi.Host.csproj", "BloomFilter.HttpApi.Host/"]
COPY ["BloomFilter.HttpApi/BloomFilter.HttpApi.csproj", "BloomFilter.HttpApi/"]
COPY ["BloomFilter.Business/BloomFilter.Business.csproj", "BloomFilter.Business/"]
COPY ["BloomFilter.DataAccess/BloomFilter.DataAccess.csproj", "BloomFilter.DataAccess/"]
COPY ["BloomFilter.Dto/BloomFilter.Dto.csproj", "BloomFilter.Dto/"]
COPY ["BloomFilter.Entity/BloomFilter.Entity.csproj", "BloomFilter.Entity/"]
COPY ["BloomFilter.Shared/BloomFilter.Shared.csproj", "BloomFilter.Shared/"]

RUN dotnet restore "BloomFilter.HttpApi.Host/BloomFilter.HttpApi.Host.csproj"

COPY . .
WORKDIR "/src/BloomFilter.HttpApi.Host"
RUN dotnet build "BloomFilter.HttpApi.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BloomFilter.HttpApi.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "BloomFilter.HttpApi.Host.dll"]