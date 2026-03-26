namespace ScreenDrafts.Modules.Drafts.Composition;

public static class DraftsModule
{
  public static IServiceCollection AddDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDraftsFeatures();

    services.AddTypeHandler();

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddDraftsInfrastructure(configuration);

    return services;
  }

  private static IServiceCollection AddTypeHandler(this IServiceCollection services)
  {
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftPartStatus>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftStatus>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftType>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<HostRole>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<ParticipantKind>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<ReleaseChannel>());
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

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<MediaAddedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<MediaFetchedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);
  }

  private static IServiceCollection AddDraftsFeatures(this IServiceCollection services)
  {
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DraftsUnitOfWorkBehavior<,>));
    services.AddScoped<ParticipantResolver>();
    services.AddScoped<DraftBoardParticipantResolver>();
    services.AddScoped<IAdministrationIntegrationEventDispatcher, DraftsIntegrationEventDispatcher>();
    services.AddScoped<IDraftsDomainEventDispatcher, DraftsDomainEventDispatcher>();
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
