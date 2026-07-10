namespace ScreenDrafts.Modules.RealTimeUpdates.Composition;

public static class RealTimeUpdatesModule
{
  private static readonly string _moduleName = typeof(RealTimeUpdatesModule)
    .Assembly.GetName()
    .Name!;

  public static IServiceCollection AddRealTimeUpdatesModule(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddSignalR();

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddRealTimeUpdatesInfrastructure(configuration);

    services.AddRealTimeUpdatesFeatures(configuration);

    return services;
  }

  private static void AddRealTimeUpdatesFeatures(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.Configure<RealTimeUpdatesDraftsOptions>(
      configuration.GetSection(RealTimeUpdatesDraftsOptions.SectionName)
    );
    services.AddScoped<
      IRealTimeUpdatesDomainEventDispatcher,
      RealTimeUpdatesDomainEventDispatcher
    >();
    services.AddScoped<
      IRealTimeUpdatesIntegrationEventDispatcher,
      RealTimeUpdatesIntegrationEventDispatcher
    >();
  }

  public static void ConfigureConsumers(
    IRegistrationConfigurator registrationConfigurator,
    string instanceId
  )
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    var moduleInstanceId = $"{instanceId}-{_moduleName.ToLowerInvariant()}";

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<PickAddedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<PickRevealedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<PickSubmittedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<VetoAppliedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<VetoOverrideAppliedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<CommissionerOverrideAppliedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DraftPositionAssignedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DraftPositionUnassignedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DrafterHonorificEarnedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<MovieHonorificEarnedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<VetoUndoneIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<PositionsSetIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<PickUndoneIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DraftCompletedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DraftPartCompletedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<CommunityRuleAppliedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);
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
