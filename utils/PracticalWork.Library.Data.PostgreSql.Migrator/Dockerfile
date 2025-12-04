FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["utils/PracticalWork.Library.Data.PostgreSql.Migrator/PracticalWork.Library.Data.PostgreSql.Migrator.csproj", "utils/PracticalWork.Library.Data.PostgreSql.Migrator/"]
COPY ["src/PracticalWork.Library.Data.PostgreSql/PracticalWork.Library.Data.PostgreSql.csproj", "src/PracticalWork.Library.Data.PostgreSql/"]
COPY ["src/PracticalWork.Library/PracticalWork.Library.csproj", "src/PracticalWork.Library/"]
COPY ["src/PracticalWork.Library.Contracts/PracticalWork.Library.Contracts.csproj", "src/PracticalWork.Library.Contracts/"]
RUN dotnet restore "utils/PracticalWork.Library.Data.PostgreSql.Migrator/PracticalWork.Library.Data.PostgreSql.Migrator.csproj"
COPY . .
WORKDIR "/src/utils/PracticalWork.Library.Data.PostgreSql.Migrator"
RUN dotnet build "PracticalWork.Library.Data.PostgreSql.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PracticalWork.Library.Data.PostgreSql.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PracticalWork.Library.Data.PostgreSql.Migrator.dll"]
