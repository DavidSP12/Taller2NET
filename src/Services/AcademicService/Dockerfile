FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/AcademicService/AcademicService.csproj", "Services/AcademicService/"]
COPY ["src/Shared/Taller2NET.Shared/Taller2NET.Shared.csproj", "Shared/Taller2NET.Shared/"]
RUN dotnet restore "Services/AcademicService/AcademicService.csproj"

COPY src/Services/AcademicService/ Services/AcademicService/
COPY src/Shared/Taller2NET.Shared/ Shared/Taller2NET.Shared/
WORKDIR "/src/Services/AcademicService"
RUN dotnet build "AcademicService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcademicService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AcademicService.dll"]
