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
COPY ["src/modules/users/ScreenDrafts.Modules.Administration.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.Administration.Application/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Administration.Domain/ScreenDrafts.Modules.Administration.Domain.csproj", "src/modules/users/ScreenDrafts.Modules.Administration.Domain/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Administration.Infrastructure/ScreenDrafts.Modules.Administration.Infrastructure.csproj", "src/modules/users/ScreenDrafts.Modules.Administration.Infrastructure/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Administration.Integration/ScreenDrafts.Modules.Administration.Integration.csproj", "src/modules/users/ScreenDrafts.Modules.Administration.Integration/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Administration.Presentation/ScreenDrafts.Modules.Administration.Presentation.csproj", "src/modules/users/ScreenDrafts.Modules.Administration.Presentation/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Audit.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.Audit.Application/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Audit.Domain/ScreenDrafts.Modules.Audit.Domain.csproj", "src/modules/users/ScreenDrafts.Modules.Audit.Domain/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Audit.Infrastructure/ScreenDrafts.Modules.Audit.Infrastructure.csproj", "src/modules/users/ScreenDrafts.Modules.Audit.Infrastructure/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Audit.Integration/ScreenDrafts.Modules.Audit.Integration.csproj", "src/modules/users/ScreenDrafts.Modules.Audit.Integration/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Audit.Presentation/ScreenDrafts.Modules.Audit.Presentation.csproj", "src/modules/users/ScreenDrafts.Modules.Audit.Presentation/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Communications.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.Communications.Application/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Communications.Domain/ScreenDrafts.Modules.Communications.Domain.csproj", "src/modules/users/ScreenDrafts.Modules.Communications.Domain/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Communications.Infrastructure/ScreenDrafts.Modules.Communications.Infrastructure.csproj", "src/modules/users/ScreenDrafts.Modules.Communications.Infrastructure/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Communications.Integration/ScreenDrafts.Modules.Communications.Integration.csproj", "src/modules/users/ScreenDrafts.Modules.Communications.Integration/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Communications.Presentation/ScreenDrafts.Modules.Communications.Presentation.csproj", "src/modules/users/ScreenDrafts.Modules.Communications.Presentation/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Application/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Domain/ScreenDrafts.Modules.Drafts.Domain.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Domain/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Infrastructure/ScreenDrafts.Modules.Drafts.Infrastructure.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Infrastructure/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.IntegrationEvents/ScreenDrafts.Modules.Drafts.IntegrationEvents.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.IntegrationEvents/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Presentation/ScreenDrafts.Modules.Drafts.Presentation.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Presentation/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Integrations.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.Integrations.Application/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Integrations.Domain/ScreenDrafts.Modules.Integrations.Domain.csproj", "src/modules/users/ScreenDrafts.Modules.Integrations.Domain/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Integrations.Infrastructure/ScreenDrafts.Modules.Integrations.Infrastructure.csproj", "src/modules/users/ScreenDrafts.Modules.Integrations.Infrastructure/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Integrations.Integration/ScreenDrafts.Modules.Integrations.Integration.csproj", "src/modules/users/ScreenDrafts.Modules.Integrations.Integration/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Integrations.Presentation/ScreenDrafts.Modules.Integrations.Presentation.csproj", "src/modules/users/ScreenDrafts.Modules.Integrations.Presentation/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Movies.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.Movies.Application/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Movies.Domain/ScreenDrafts.Modules.Movies.Domain.csproj", "src/modules/users/ScreenDrafts.Modules.Movies.Domain/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Movies.Infrastructure/ScreenDrafts.Modules.Movies.Infrastructure.csproj", "src/modules/users/ScreenDrafts.Modules.Movies.Infrastructure/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Movies.Integration/ScreenDrafts.Modules.Movies.Integration.csproj", "src/modules/users/ScreenDrafts.Modules.Movies.Integration/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Movies.Presentation/ScreenDrafts.Modules.Movies.Presentation.csproj", "src/modules/users/ScreenDrafts.Modules.Movies.Presentation/"]
COPY ["src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Application/"]
COPY ["src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Domain/ScreenDrafts.Modules.RealTimeUpdates.Domain.csproj", "src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Domain/"]
COPY ["src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Infrastructure/ScreenDrafts.Modules.RealTimeUpdates.Infrastructure.csproj", "src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Infrastructure/"]
COPY ["src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Integration/ScreenDrafts.Modules.RealTimeUpdates.Integration.csproj", "src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Integration/"]
COPY ["src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Presentation/ScreenDrafts.Modules.RealTimeUpdates.Presentation.csproj", "src/modules/users/ScreenDrafts.Modules.RealTimeUpdates.Presentation/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Reporting.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.Reporting.Application/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Reporting.Domain/ScreenDrafts.Modules.Reporting.Domain.csproj", "src/modules/users/ScreenDrafts.Modules.Reporting.Domain/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Reporting.Infrastructure/ScreenDrafts.Modules.Reporting.Infrastructure.csproj", "src/modules/users/ScreenDrafts.Modules.Reporting.Infrastructure/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Reporting.Integration/ScreenDrafts.Modules.Reporting.Integration.csproj", "src/modules/users/ScreenDrafts.Modules.Reporting.Integration/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Reporting.Presentation/ScreenDrafts.Modules.Reporting.Presentation.csproj", "src/modules/users/ScreenDrafts.Modules.Reporting.Presentation/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Users.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.Users.Application/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Users.Domain/ScreenDrafts.Modules.Users.Domain.csproj", "src/modules/users/ScreenDrafts.Modules.Users.Domain/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Users.Infrastructure/ScreenDrafts.Modules.Users.Infrastructure.csproj", "src/modules/users/ScreenDrafts.Modules.Users.Infrastructure/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Users.Integration/ScreenDrafts.Modules.Users.Integration.csproj", "src/modules/users/ScreenDrafts.Modules.Users.Integration/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Users.Presentation/ScreenDrafts.Modules.Users.Presentation.csproj", "src/modules/users/ScreenDrafts.Modules.Users.Presentation/"]
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