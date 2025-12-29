FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/PracticalWork.Library.Web/PracticalWork.Library.Web.csproj", "PracticalWork.Library.Web/"]
COPY ["src/PracticalWork.Library/PracticalWork.Library.csproj", "PracticalWork.Library/"]
COPY ["src/PracticalWork.Library.Contracts/PracticalWork.Library.Contracts.csproj", "PracticalWork.Library.Contracts/"]
COPY ["src/PracticalWork.Library.Controllers/PracticalWork.Library.Controllers.csproj", "PracticalWork.Library.Controllers/"]
COPY ["src/PracticalWork.Library.Cache.Redis/PracticalWork.Library.Cache.Redis.csproj", "PracticalWork.Library.Cache.Redis/"]
COPY ["src/PracticalWork.Library.Data.Minio/PracticalWork.Library.Data.Minio.csproj", "PracticalWork.Library.Data.Minio/"]
COPY ["src/PracticalWork.Library.Data.PostgreSql/PracticalWork.Library.Data.PostgreSql.csproj", "PracticalWork.Library.Data.PostgreSql/"]
COPY ["src/PracticalWork.Library.Data.PostgreSql/PracticalWork.Library.Data.Report.csproj", "PracticalWork.Library.Report.PostgreSql/"]
COPY ["src/PracticalWork.Library.Data.PostgreSql/PracticalWork.Library.MessageBroker.csproj", "PracticalWork.Library.MessageBroker/"]

RUN dotnet restore "PracticalWork.Library.Web/PracticalWork.Library.Web.csproj"

COPY . .

WORKDIR "/src/PracticalWork.Library.Web"
RUN dotnet build "PracticalWork.Library.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PracticalWork.Library.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
