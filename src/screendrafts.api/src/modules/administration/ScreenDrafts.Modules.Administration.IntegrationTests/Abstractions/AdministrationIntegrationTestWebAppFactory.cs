namespace ScreenDrafts.Modules.Administration.IntegrationTests.Abstractions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
public sealed class AdministrationIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  private KeycloakContainer? _keycloakContainer;
  private bool _keycloakInitialized;

  public AdministrationIntegrationTestWebAppFactory() : base()
  {
    EnsureKeyCloakInitialized();
  }

  private void EnsureKeyCloakInitialized()
  {
    if (_keycloakInitialized)
    {
      return;
    }

    var realmFilePath = Path.Combine(AppContext.BaseDirectory, "screendrafts-realm-export.json");
    var realmFileExists = File.Exists(realmFilePath);

    var builder = new KeycloakBuilder("quay.io/keycloak/keycloak:26.1.0")
      .WithPortBinding(9000, true);

    if (realmFileExists)
    {
      builder = builder
        .WithResourceMapping(
          new FileInfo(realmFilePath),
          new FileInfo("/opt/keycloak/data/import/realm.json"))
        .WithCommand("--import-realm");
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
      ["Authentication__MetadataAddress"] = $"{keyCloakRealmUrl}/.well-known/openid-configuration",
      ["Authentication__TokenValidationParameters__ValidIssuers"] = keyCloakRealmUrl,
      ["Authentication__RequireHttpsMetadata"] = "false",
      ["Authentication__Audience"] = "account",
      ["KeyCloak__HealthUrl"] = $"{keycloakAddress}health",
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
    base.ConfigureWebHost(builder);
  }

  protected override IEnumerable<Type> GetDbContextTypes()
  {
    var needed = new[]
    {
      "ScreenDrafts.Modules.Administration.Infrastructure.Database.AdministrationDbContext",
      "ScreenDrafts.Modules.Users.Infrastructure.Database.UsersDbContext",
    };

    return [.. AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => a.GetTypes())
      .Where(t => needed.Contains(t.FullName, StringComparer.Ordinal))];
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

  protected override async Task ApplyMigrationsAsync()
  {
    await base.ApplyMigrationsAsync();
    await CreateAdministrationTablesAsync();
  }

  private async Task CreateAdministrationTablesAsync()
  {
    using var scope = Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AdministrationDbContext>();

    await dbContext.Database.ExecuteSqlRawAsync(
      """
      CREATE TABLE IF NOT EXISTS administration.permissions (
          code VARCHAR(100) NOT NULL,
          CONSTRAINT pk_permissions PRIMARY KEY (code)
      );

      CREATE TABLE IF NOT EXISTS administration.roles (
          name VARCHAR(50) NOT NULL,
          CONSTRAINT pk_roles PRIMARY KEY (name)
      );

      CREATE TABLE IF NOT EXISTS administration.role_permissions (
          permission_code VARCHAR(100) NOT NULL,
          role_name       VARCHAR(50)  NOT NULL,
          CONSTRAINT pk_role_permissions PRIMARY KEY (permission_code, role_name),
          CONSTRAINT fk_role_permissions_permission_code
              FOREIGN KEY (permission_code) REFERENCES administration.permissions (code)
              ON DELETE CASCADE,
          CONSTRAINT fk_role_permissions_role_name
              FOREIGN KEY (role_name) REFERENCES administration.roles (name)
              ON DELETE CASCADE
      );

      CREATE INDEX IF NOT EXISTS ix_role_permissions_role_name
          ON administration.role_permissions (role_name);

      CREATE TABLE IF NOT EXISTS administration.user_roles (
          user_id   UUID        NOT NULL,
          role_name VARCHAR(50) NOT NULL,
          CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_name),
          CONSTRAINT fk_user_roles_role_name
              FOREIGN KEY (role_name) REFERENCES administration.roles (name)
              ON DELETE CASCADE
      );

      CREATE INDEX IF NOT EXISTS ix_user_roles_user_id
          ON administration.user_roles (user_id);
      """);
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
