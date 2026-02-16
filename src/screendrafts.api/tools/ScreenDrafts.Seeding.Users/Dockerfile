FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .
RUN dotnet restore "./tools/ScreenDrafts.Seeding.Users/ScreenDrafts.Seeding.Users.csproj"
RUN dotnet publish "./tools/ScreenDrafts.Seeding.Users/ScreenDrafts.Seeding.Users.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update \
  && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
  && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "ScreenDrafts.Seeding.Users.dll"]