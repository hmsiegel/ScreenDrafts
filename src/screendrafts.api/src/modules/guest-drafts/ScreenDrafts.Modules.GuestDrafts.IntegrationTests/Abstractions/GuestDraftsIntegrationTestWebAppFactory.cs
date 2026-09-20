using ScreenDrafts.Common.Infrastructure.Identity;
using Testcontainers.Keycloak;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Globalization",
  "CA1303:Do not pass literals as localized parameters",
  Justification = "<Pending>"
)]
public class GuestDraftsIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  private KeycloakContainer? _keycloakContainer;
  private bool _keycloakInitialized;

  public GuestDraftsIntegrationTestWebAppFactory()
    : base()
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

    var realmFilePath = Path.Combine(AppContext.BaseDirectory, "screendrafts-realm-export.json");
    var realmFileExists = File.Exists(realmFilePath);

    Console.WriteLine($"Realm file path: {realmFilePath}");
    Console.WriteLine($"Realm file exists: {realmFileExists}");

    var builder = new KeycloakBuilder("quay.io/keycloak/keycloak:26.1.0").WithPortBinding(
      9000,
      true
    );

    if (realmFileExists)
    {
      Console.WriteLine("Configuring KeyCloak with realm import...");
      builder = builder
        .WithResourceMapping(
          new FileInfo(realmFilePath),
          new FileInfo("/opt/keycloak/data/import/realm.json")
        )
        .WithCommand("--import-realm");
    }
    else
    {
      Console.WriteLine("Realm file not found - starting KeyCloak without pre-configured realm.");
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

    // The shared IntegrationTestWebAppFactory.SetEnvironmentVariables() only sets
    // ConnectionStrings__<Module> for the modules that existed when it was written
    // -- GuestDrafts isn't among them, so ConnectionStrings:GuestDrafts is never
    // set and GuestDraftsDbContext fails to resolve it. Every module actually
    // shares one physical Postgres database/container (split only by schema), so
    // this just points GuestDrafts at the same connection string every other
    // module already got from ConnectionStrings__Database.
    var databaseConnectionString = Environment.GetEnvironmentVariable(
      "ConnectionStrings__Database"
    );
    if (!string.IsNullOrEmpty(databaseConnectionString))
    {
      Environment.SetEnvironmentVariable(
        "ConnectionStrings__GuestDrafts",
        databaseConnectionString
      );
    }

    builder.UseUrls("http://localhost:0");
    builder.UseWebRoot(Path.GetTempPath());

    base.ConfigureWebHost(builder);
  }

  protected override void ConfigureModuleServices(IServiceCollection services)
  {
    base.ConfigureModuleServices(services);

    services.RemoveAll<IUsersApi>();
    services.AddSingleton<FakeUsersApi>();
    services.AddSingleton<IUsersApi>(sp => sp.GetRequiredService<FakeUsersApi>());

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
    // Only migrate the schema needed for GuestDrafts tests. IUsersApi is faked
    // (see ConfigureModuleServices), and movies are cached locally in
    // guest_drafts.movies, so neither the Users nor the Movies schema needs to
    // exist for these tests to run.
    var needed = new[]
    {
      "ScreenDrafts.Modules.GuestDrafts.Infrastructure.Database.GuestDraftsDbContext",
    };

    return
    [
      .. AppDomain
        .CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => needed.Contains(t.FullName, StringComparer.Ordinal)),
    ];
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
