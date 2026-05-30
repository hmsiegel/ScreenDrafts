namespace ScreenDrafts.Modules.Reporting.Composition;

public static class ReportingModule
{
  private static readonly string _moduleName = typeof(ReportingModule).Assembly.GetName().Name!;

  public static IServiceCollection AddReportingModule(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddReportingInfratsructure(configuration);

    services.AddReportingFeatures();

    return services;
  }

  public static IServiceCollection AddReportingSeeding(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.AddReportingInfratsructure(configuration);
    return services;
  }

  private static void AddReportingFeatures(this IServiceCollection services)
  {
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ReportingUnitOfWorkBehavior<,>));
    services.AddScoped<IReportingIntegrationEventDispatcher, ReportingIntegrationEventDispatcher>();
    services.AddScoped<IReportingDomainEventDispatcher, ReportingDomainEventDispatcher>();
  }

  public static void ConfigureConsumers(
    IRegistrationConfigurator registrationConfigurator,
    string instanceId
  )
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    var moduleInstanceId = $"{instanceId}-{_moduleName.ToLowerInvariant()}";

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DraftPartStartedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<PickLockedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DraftCompletedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DraftPartCompletedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<DraftPartReleaseAddedIntegrationEvent>>()
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
