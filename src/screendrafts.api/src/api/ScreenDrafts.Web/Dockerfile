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
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Application/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Domain/ScreenDrafts.Modules.Drafts.Domain.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Domain/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Infrastructure/ScreenDrafts.Modules.Drafts.Infrastructure.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Infrastructure/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Integration/ScreenDrafts.Modules.Drafts.Integration.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Integration/"]
COPY ["src/modules/drafts/ScreenDrafts.Modules.Drafts.Presentation/ScreenDrafts.Modules.Drafts.Presentation.csproj", "src/modules/drafts/ScreenDrafts.Modules.Drafts.Presentation/"]
COPY ["src/modules/users/ScreenDrafts.Modules.Users.Application/ScreenDrafts.Modules.Drafts.Application.csproj", "src/modules/users/ScreenDrafts.Modules.Drafts.Application/"]
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