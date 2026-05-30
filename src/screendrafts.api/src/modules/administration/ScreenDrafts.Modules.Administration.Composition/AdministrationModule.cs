namespace ScreenDrafts.Modules.Administration.Composition;

public static class AdministrationModule
{
  private static readonly string _moduleName = typeof(AdministrationModule)
    .Assembly.GetName()
    .Name!;

  public static IServiceCollection AddAdministrationModule(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddAdministrationInfrastructure(configuration);

    services.AddAdministrationFeatures();

    return services;
  }

  public static void ConfigureConsumers(
    IRegistrationConfigurator registrationConfigurator,
    string instanceId
  )
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    var moduleInstanceId = $"{instanceId}-{_moduleName.ToLowerInvariant()}";

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<UserRegisteredIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DrafterCreatedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<HostCreatedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);
  }

  private static void AddAdministrationFeatures(this IServiceCollection services)
  {
    services.AddScoped<IAdministrationApi, AdministrationApi>();
    services.AddScoped<
      IAdministrationIntegrationEventDispatcher,
      AdministrationIntegrationEventDispatcher
    >();
    services.AddScoped<IAdministrationDomainEventDispatcher, AdministrationDomainEventDispatcher>();
  }

  private static void AddDomainEventHandlers(this IServiceCollection services)
  {
    Type[] domainEventHandlers =
    [
      .. AssemblyReference
        .Assembly.GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler))),
    ];

    foreach (Type domainEventHandler in domainEventHandlers)
    {
      services.TryAddScoped(domainEventHandler);

      Type domainEvent = domainEventHandler
        .GetInterfaces()
        .Single(i => i.IsGenericType)
        .GetGenericArguments()
        .Single();

      Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(
        domainEvent
      );

      services.Decorate(domainEventHandler, closedIdempotentHandler);
    }
  }

  private static void AddIntegrationEventHandlers(this IServiceCollection services)
  {
    Type[] integrationEventHandlers =
    [
      .. AssemblyReference
        .Assembly.GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler))),
    ];

    foreach (Type integrationEventHandler in integrationEventHandlers)
    {
      services.TryAddScoped(integrationEventHandler);

      Type integrationEvent = integrationEventHandler
        .GetInterfaces()
        .Single(i => i.IsGenericType)
        .GetGenericArguments()
        .Single();

      Type closedIdempotentHandler = typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(
        integrationEvent
      );

      services.Decorate(integrationEventHandler, closedIdempotentHandler);
    }
  }
}
