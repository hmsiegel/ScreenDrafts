FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["src/api/ScreenDrafts.Web/ScreenDrafts.Web.csproj", "src/api/ScreenDrafts.Web/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts./ScreenDrafts.Modules.Drafts.Application.csproj", "src/api/ScreenDrafts.Modules.Drafts.Application/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts./ScreenDrafts.Modules.Drafts.Domain.csproj", "src/api/ScreenDrafts.Modules.Drafts.Domain/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts./ScreenDrafts.Modules.Drafts.Infrastructure.csproj", "src/api/ScreenDrafts.Modules.Drafts.Infrastructure/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts./ScreenDrafts.Modules.Drafts.Integration.csproj", "src/api/ScreenDrafts.Modules.Drafts.Integration/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts./ScreenDrafts.Modules.Drafts.Presentation.csproj", "src/api/ScreenDrafts.Modules.Drafts.Presentation/"]
RUN dotnet restore "./src/api/ScreenDrafts.Web/ScreenDrafts.Web.csproj"
COPY . .
WORKDIR "/src/src/api/ScreenDrafts.Web"
RUN dotnet build "./ScreenDrafts.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScreenDrafts.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScreenDrafts.Web.dll"]