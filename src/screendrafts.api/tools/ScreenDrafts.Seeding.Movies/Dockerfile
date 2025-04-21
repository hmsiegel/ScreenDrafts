FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .
RUN dotnet restore "./tools/ScreenDrafts.Seeding.Movies/ScreenDrafts.Seeding.Movies.csproj"
RUN dotnet publish "./tools/ScreenDrafts.Seeding.Movies/ScreenDrafts.Seeding.Movies.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "ScreenDrafts.Seeding.Movies.dll"]