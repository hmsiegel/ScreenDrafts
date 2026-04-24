using ScreenDrafts.Modules.Drafts.IntegrationEvents;
using ScreenDrafts.Modules.Integrations.Domain.Zoom;
using ScreenDrafts.Modules.Integrations.Infrastructure.Zoom;

namespace ScreenDrafts.Modules.Integrations.Composition;

public static class IntegrationsModule
{
  public static IServiceCollection AddIntegrationsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddIntegrationsInfrastructure(configuration);

    services.AddIntegrationFeatures(configuration);

    return services;
  }

  public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<FetchMediaRequestedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<StartZoomRecordingRequestedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<StopZoomRecordingRequestedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);
  }

  public static void AddIntegrationFeatures(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddScoped<IImdbService, ImdbService>();
    services.AddScoped<IOmdbService, OmdbService>();

    services.Configure<TmdbSettings>(configuration.GetSection(TmdbSettings.SectionName));
    services.Configure<IgdbSettings>(configuration.GetSection(IgdbSettings.SectionName));

    services.AddHttpClient<ITmdbService, TmdbService>((sp, client) =>
    {
      TmdbSettings tmdbSettings = sp.GetRequiredService<IOptions<TmdbSettings>>().Value;
      client.BaseAddress = new Uri(tmdbSettings.BaseAddress);
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tmdbSettings.AccessToken);
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    });

    services.AddHttpClient<IIgdbService, IgdbService>((sp, client) =>
    {
      IgdbSettings igdbSettings = sp.GetRequiredService<IOptions<IgdbSettings>>().Value;
      client.BaseAddress = new Uri(igdbSettings.BaseAddress);
    });

    services.Configure<ZoomSettings>(configuration.GetSection(ZoomSettings.SectionName));

    services.AddHttpClient<IZoomApiClient, ZoomApiClient>((sp, client) =>
    {
      var settings = sp.GetRequiredService<IOptions<ZoomSettings>>().Value;
      client.BaseAddress = new Uri(settings.BaseAddress);
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    });

    services.AddScoped<IIntegrationsApi, IntegrationsApi>();
    services.AddScoped<IIntegrationsIntegrationEventDispatcher, IntegrationsIntegrationEventDispatcher>();
    services.AddScoped<IIntegrationsDomainEventDispatcher, IntegrationsDomainEventDispatcher>();
  }

  private static void AddDomainEventHandlers(this IServiceCollection services)
  {
    Type[] domainEventHandlers = [.. AssemblyReference.Assembly
        .GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))];

    foreach (Type domainEventHandler in domainEventHandlers)
    {
      services.TryAddScoped(domainEventHandler);

      Type domainEvent = domainEventHandler
          .GetInterfaces()
          .Single(i => i.IsGenericType)
          .GetGenericArguments()
          .Single();

      Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);

      services.Decorate(domainEventHandler, closedIdempotentHandler);
    }
  }

  private static void AddIntegrationEventHandlers(this IServiceCollection services)
  {
    Type[] integrationEventHandlers = [.. AssemblyReference.Assembly
        .GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))];

    foreach (Type integrationEventHandler in integrationEventHandlers)
    {
      services.TryAddScoped(integrationEventHandler);

      Type integrationEvent = integrationEventHandler
          .GetInterfaces()
          .Single(i => i.IsGenericType)
          .GetGenericArguments()
          .Single();

      Type closedIdempotentHandler = typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

      services.Decorate(integrationEventHandler, closedIdempotentHandler);
    }
  }
}
