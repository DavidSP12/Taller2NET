FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/StatisticsService/StatisticsService.csproj", "Services/StatisticsService/"]
COPY ["src/Shared/Taller2NET.Shared/Taller2NET.Shared.csproj", "Shared/Taller2NET.Shared/"]
RUN dotnet restore "Services/StatisticsService/StatisticsService.csproj"

COPY src/Services/StatisticsService/ Services/StatisticsService/
COPY src/Shared/Taller2NET.Shared/ Shared/Taller2NET.Shared/
WORKDIR "/src/Services/StatisticsService"
RUN dotnet build "StatisticsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatisticsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8081
ENTRYPOINT ["dotnet", "StatisticsService.dll"]
