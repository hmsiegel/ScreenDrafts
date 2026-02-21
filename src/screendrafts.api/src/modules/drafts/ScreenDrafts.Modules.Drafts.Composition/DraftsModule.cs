namespace ScreenDrafts.Modules.Drafts.Composition;

public static class DraftsModule
{
  public static IServiceCollection AddDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDraftsFeatures();

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddDraftsInfrastructure(configuration);

    return services;
  }

  public static IServiceCollection AddDraftsSeeding(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDraftsInfrastructure(configuration);

    return services;
  }

  public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<UserRegisteredIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<MovieAddedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);
  }

  private static IServiceCollection AddDraftsFeatures(this IServiceCollection services)
  {
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DraftsUnitOfWorkBehavior<,>));
    SqlMapper.AddTypeHandler(new JsonTypeHandler<IReadOnlyList<DraftPositionResponse>>());
    services.AddScoped<ParticipantResolver>();
    services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
    services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    return services;
  }

  private static void AddDomainEventHandlers(this IServiceCollection services)
  {
    Type[] domainEventHandlers = [.. Features.AssemblyReference.Assembly
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
    Type[] integrationEventHandlers = [.. Features.AssemblyReference.Assembly
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
