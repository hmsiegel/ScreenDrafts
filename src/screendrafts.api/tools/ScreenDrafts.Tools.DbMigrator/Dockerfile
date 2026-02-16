FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .
RUN dotnet restore "./tools/ScreenDrafts.Tools.DbMigrator/ScreenDrafts.Tools.DbMigrator.csproj"

RUN dotnet publish "./tools/ScreenDrafts.Tools.DbMigrator/ScreenDrafts.Tools.DbMigrator.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ScreenDrafts.Tools.DbMigrator.dll"]