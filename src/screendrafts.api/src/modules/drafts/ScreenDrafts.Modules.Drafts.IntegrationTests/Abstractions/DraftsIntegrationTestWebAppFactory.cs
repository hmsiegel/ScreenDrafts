using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ScreenDrafts.Common.Application.EventBus;
using ScreenDrafts.Modules.Communications.Domain.Email;
using ScreenDrafts.Modules.RealTimeUpdates.Features;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
public class DraftsIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  private KeycloakContainer? _keycloakContainer;
  private bool _keycloakInitialized;

  /// <summary>
  /// Shared in-memory email capture. Tests call <see cref="FakeEmailCapture.Clear"/> in setup
  /// and inspect <see cref="FakeEmailCapture.SentEmails"/> after outbox drain.
  /// </summary>
  public FakeEmailCapture EmailCapture { get; } = new FakeEmailCapture();

  /// <summary>
  /// Records integration events published by Drafts domain event handlers.
  /// Call <see cref="CapturingEventBus.Clear"/> in setup, then
  /// <see cref="Helpers.DraftScenarioBase.DispatchIntegrationEventsAsync"/> to deliver
  /// captured events in-process to Communications and RealTimeUpdates consumers.
  /// </summary>
  public CapturingEventBus EventBusCapture { get; } = new CapturingEventBus();

  /// <summary>
  /// Captures SignalR messages sent through DraftHub by RealTimeUpdates consumers.
  /// Inspect after <see cref="Helpers.DraftScenarioBase.DispatchIntegrationEventsAsync"/>.
  /// </summary>
  public DraftHubCapture HubCapture { get; } = new DraftHubCapture();

  public DraftsIntegrationTestWebAppFactory() : base()
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

    // Replace the real email service with the in-memory capture for all tests.
    services.RemoveAll<IEmailService>();
    services.AddSingleton<IEmailService>(EmailCapture);
    services.AddSingleton<IEmailCapture>(EmailCapture);

    // Replace the base factory's NoOpEventBus with a capturing version so that
    // integration events published by Drafts domain event handlers can be dispatched
    // in-process to Communications and RealTimeUpdates consumers during scenario tests.
    services.RemoveAll<IEventBus>();
    services.AddSingleton<IEventBus>(EventBusCapture);
    services.AddSingleton<CapturingEventBus>(EventBusCapture);

    // Replace the real SignalR hub context so that RealTimeUpdates consumers resolved
    // in-process write their broadcasts to DraftHubCapture instead of a real hub.
    services.RemoveAll<IHubContext<DraftHub>>();
    services.AddSingleton<IHubContext<DraftHub>>(HubCapture);

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
    return [typeof(DraftsDbContext), typeof(UsersDbContext)];
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
