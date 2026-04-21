using Dapper;

namespace ScreenDrafts.Modules.Users.IntegrationTests.Abstractions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
public class UsersIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  private KeycloakContainer? _keycloakContainer;
  private bool _keycloakInitialized;

  public UsersIntegrationTestWebAppFactory() : base()
  {
    EnsureKeyCloakInitialized();
  }

  private void EnsureKeyCloakInitialized()
  {
    if (_keycloakInitialized)
    {
      return;
    }

    Console.WriteLine("Starting KeyCloak container...");

    // Check if realm export file exists
    var realmFilePath = Path.Combine(AppContext.BaseDirectory, "screendrafts-realm-export.json");
    var realmFileExists = File.Exists(realmFilePath);

    Console.WriteLine($"Realm file path: {realmFilePath}");
    Console.WriteLine($"Realm file exists: {realmFileExists}");

    var builder = new KeycloakBuilder()
      .WithImage("quay.io/keycloak/keycloak:26.1.0")
      .WithPortBinding(9000, true);

    // Only configure resource mapping if the realm file exists
    if (realmFileExists)
    {
      Console.WriteLine("Configuring KeyCloak with realm import...");
      builder = builder
        .WithResourceMapping(
          new FileInfo(realmFilePath),
          new FileInfo("/opt/keycloak/data/import/realm.json"))
        .WithCommand("--import-realm");
    }
    else
    {
      Console.WriteLine("Realm file not found - starting KeyCloak without pre-configured realm.");
      Console.WriteLine("Note: You may need to configure the realm manually or create the realm export file.");
    }

    _keycloakContainer = builder.Build();

    _keycloakContainer.StartAsync().Wait();

    SetKeyCloakEnvironmentVariables();
    _keycloakInitialized = true;

  }

  private void SetKeyCloakEnvironmentVariables()
  {
    if (_keycloakContainer is null)
    {
      return;
    }

    var keycloakAddress = _keycloakContainer.GetBaseAddress();
    var keyCloakRealmUrl = $"{keycloakAddress}realms/screendrafts";

    var envVars = new Dictionary<string, string>
    {
      // Authentication settings
      ["Authentication__MetadataAddress"] = $"{keyCloakRealmUrl}/.well-known/openid-configuration",
      ["Authentication__TokenValidationParameters__ValidIssuers"] = keyCloakRealmUrl,
      ["Authentication__RequireHttpsMetadata" ] = "false",
      ["Authentication__Audience"] = "account",

      // KeyCloak Health Url
      ["KeyCloak__HealthUrl"] = $"{keycloakAddress}health",

      // Users module KeyCloak settings
      ["Users__KeyCloak__AdminUrl"] = $"{keycloakAddress}admin/realms/screendrafts/",
      ["Users__KeyCloak__TokenUrl"] = $"{keyCloakRealmUrl}/protocol/openid-connect/token",
      ["Users__KeyCloak__ConfidentialClientId"] = "screendrafts-confidential-client",
      ["Users__KeyCloak__ConfidentialClientSecret"] = "oRL4la55pi1uMlJMKSlg3hrhLfvKrZsg",
      ["Users__KeyCloak__PublicClientId"] = "screendrafts-public-client",
    };

    foreach (var (key, value) in envVars)
    {
      Environment.SetEnvironmentVariable(key, value);
    }

  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    EnsureKeyCloakInitialized();

    builder.UseUrls("http://localhost:0");

    Console.WriteLine("Configuring web host for tests...");

    base.ConfigureWebHost(builder);
  }

  protected override void ConfigureModuleServices(IServiceCollection services)
  {
    base.ConfigureModuleServices(services);

    if (_keycloakContainer is null)
    {
      return;
    }

    var keycloakAddress = _keycloakContainer.GetBaseAddress();
    var keyCloakRealmUrl = $"{keycloakAddress}realms/screendrafts";

    services.Configure<KeyCloakOptions>(o =>
    {
      o.AdminUrl = $"{keycloakAddress}admin/realms/screendrafts/";
      o.TokenUrl = $"{keyCloakRealmUrl}/protocol/openid-connect/token";
      o.ConfidentialClientId = "screendrafts-confidential-client";
      o.ConfidentialClientSecret = "oRL4la55pi1uMlJMKSlg3hrhLfvKrZsg";
      o.PublicClientId = "screendrafts-public-client";
    });
  }

  protected override IEnumerable<Type> GetDbContextTypes()
  {
    var auditContextType = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => a.GetTypes())
      .FirstOrDefault(t => string.Equals(
        t.FullName,
        "ScreenDrafts.Modules.Audit.Infrastructure.Database.AuditDbContext",
        StringComparison.Ordinal));

    return auditContextType is null
      ? [typeof(UsersDbContext)]
      : [typeof(UsersDbContext), auditContextType];
  }

  protected override async Task ApplyMigrationsAsync()
  {
    await base.ApplyMigrationsAsync();

    // The audit log tables are created by the DbMigrator SQL scripts in production,
    // not by EF migrations. Create them here so that AuditPostProcessor can write
    // to them when HTTP requests are made during tests.
    using var scope = Services.CreateScope();
    var connectionFactory = scope.ServiceProvider
      .GetRequiredService<ScreenDrafts.Common.Application.Data.IDbConnectionFactory>();

    await using var connection = await connectionFactory.OpenConnectionAsync();

    await connection.ExecuteAsync(
      """
      CREATE TABLE IF NOT EXISTS users.user_permissions
      (
          user_id         UUID         NOT NULL,
          permission_code VARCHAR(100) NOT NULL,
          CONSTRAINT pk_user_permissions PRIMARY KEY (user_id, permission_code)
      );

      CREATE INDEX IF NOT EXISTS ix_user_permissions_user_id
          ON users.user_permissions (user_id);
      """);

    await connection.ExecuteAsync(
      """
      CREATE TABLE IF NOT EXISTS audit.http_audit_logs
      (
          id               uuid        NOT NULL,
          correlation_id   uuid        NOT NULL,
          occurred_on_utc  timestamptz NOT NULL,
          actor_id         text,
          endpoint_name    text        NOT NULL,
          http_method      text        NOT NULL,
          route            text        NOT NULL,
          status_code      int,
          duration_ms      int,
          request_body     jsonb,
          response_body    jsonb,
          ip_address       text,
          CONSTRAINT pk_http_audit_logs PRIMARY KEY (id)
      );

      CREATE TABLE IF NOT EXISTS audit.domain_event_audit_logs
      (
          id               uuid        NOT NULL,
          occurred_on_utc  timestamptz NOT NULL,
          event_type       text        NOT NULL,
          source_module    text        NOT NULL,
          actor_id         text,
          entity_id        text,
          payload          jsonb       NOT NULL,
          CONSTRAINT pk_domain_event_audit_logs PRIMARY KEY (id)
      );

      CREATE TABLE IF NOT EXISTS audit.auth_audit_logs
      (
          id               uuid        NOT NULL,
          occurred_on_utc  timestamptz NOT NULL,
          event_type       text        NOT NULL,
          user_id          text,
          client_id        text,
          ip_address       text,
          details          jsonb,
          CONSTRAINT pk_auth_audit_logs PRIMARY KEY (id)
      );
      """);
  }

  protected override async Task StopContainersAsync()
  {
    if (_keycloakContainer is not null && _keycloakInitialized)
    {
      _keycloakInitialized = false;
      await _keycloakContainer.StopAsync();
      await _keycloakContainer.DisposeAsync();
      _keycloakContainer = null;
    }

    await base.StopContainersAsync();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      if (_keycloakInitialized)
      {
        Task.Run(StopContainersAsync).GetAwaiter().GetResult();
      }
      else
      {
        _keycloakContainer?.ConfigureAwait(false).DisposeAsync();
      }
    }

    base.Dispose(disposing);
  }
}
