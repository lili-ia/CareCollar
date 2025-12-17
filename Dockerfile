FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["CareCollar.Server/CareCollar.Server.csproj", "CareCollar.Server/"]
COPY ["CareCollar.Application/CareCollar.Application.csproj", "CareCollar.Application/"]
COPY ["CareCollar.Domain/CareCollar.Domain.csproj", "CareCollar.Domain/"]
COPY ["CareCollar.Infrastructure/CareCollar.Infrastructure.csproj", "CareCollar.Infrastructure/"]
COPY ["CareCollar.Persistence/CareCollar.Persistence.csproj", "CareCollar.Persistence/"]
COPY ["CareCollar.Shared/CareCollar.Shared.csproj", "CareCollar.Shared/"]

RUN dotnet restore "CareCollar.Server/CareCollar.Server.csproj"

COPY . .

WORKDIR "/src/CareCollar.Server"
RUN dotnet build "CareCollar.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CareCollar.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CareCollar.Server.dll"]