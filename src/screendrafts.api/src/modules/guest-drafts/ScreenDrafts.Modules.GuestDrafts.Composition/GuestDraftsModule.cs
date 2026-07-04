using ScreenDrafts.Modules.GuestDrafts.Infrastructure.Outbox;

namespace ScreenDrafts.Modules.GuestDrafts.Composition;

public static class GuestDraftsModule
{
  private static readonly string _moduleName = typeof(GuestDraftsModule).Assembly.GetName().Name!;

  public static IServiceCollection AddGuestDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDraftsFeatures();

    AddTypeHandler();

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddGuestDraftsInfrastructure(configuration);

    return services;
  }

  private static void AddTypeHandler()
  {
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftPartStatus>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftStatus>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftType>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<HostRole>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<ParticipantKind>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<ZoomRecordingFileType>());
  }

  public static IServiceCollection AddGuestDraftsSeeding(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.AddGuestDraftsInfrastructure(configuration);

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
      .AddConsumer<IntegrationEventConsumer<MediaAddedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<MediaFetchedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<ZoomRecordingCompletedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<UserNameUpdatedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<UserRoleAddedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<UserRoleRemovedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);

    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<CandidateListEntryAddedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);
  }

  private static void AddDraftsFeatures(this IServiceCollection services)
  {
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GuestDraftsUnitOfWorkBehavior<,>));
    services.AddScoped<
      IGuestDraftsIntegrationEventDispatcher,
      GuestDraftsIntegrationEventDispatcher
    >();
    services.AddScoped<IGuestDraftsDomainEventDispatcher, GuestDraftsDomainEventDispatcher>();
  }

  private static void AddDomainEventHandlers(this IServiceCollection services)
  {
    Type[] domainEventHandlers =
    [
      .. Features
        .AssemblyReference.Assembly.GetTypes()
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
      .. Features
        .AssemblyReference.Assembly.GetTypes()
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
