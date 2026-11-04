# The SDK stage always runs on the build machine's own CPU ($BUILDPLATFORM).
# dotnet cross-compiles for the target CPU, so the arm64 image needs no emulation here.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src

COPY . .
RUN case "$TARGETARCH" in \
      amd64) RID_ARCH=x64 ;; \
      arm64) RID_ARCH=arm64 ;; \
      *) echo "Unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
    esac \
  && dotnet restore "./tools/ScreenDrafts.Tools.DbMigrator/ScreenDrafts.Tools.DbMigrator.csproj" -a "$RID_ARCH" \
  && dotnet publish "./tools/ScreenDrafts.Tools.DbMigrator/ScreenDrafts.Tools.DbMigrator.csproj" \
       -c "$BUILD_CONFIGURATION" \
       -a "$RID_ARCH" \
       --no-restore \
       --no-self-contained \
       -o /app/publish \
       /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ScreenDrafts.Tools.DbMigrator.dll"]
