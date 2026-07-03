namespace ScreenDrafts.Modules.Drafts.Composition;

public static class DraftsModule
{
  private static readonly string _moduleName = typeof(DraftsModule).Assembly.GetName().Name!;

  public static IServiceCollection AddDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDraftsFeatures(configuration);

    services.AddTypeHandler();

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddDraftsInfrastructure(configuration);

    return services;
  }

  private static void AddTypeHandler(this IServiceCollection services)
  {
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftPartStatus>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftStatus>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<DraftType>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<HostRole>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<ParticipantKind>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<ReleaseChannel>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<SubDraftStatus>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<ZoomRecordingFileType>());
    SqlMapper.AddTypeHandler(new SmartEnumTypeHandler<CommunityFilmRuleKind>());
  }

  public static IServiceCollection AddDraftsSeeding(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.AddDraftsInfrastructure(configuration);

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

  private static void AddDraftsFeatures(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DraftsUnitOfWorkBehavior<,>));
    services.Configure<DraftsOptions>(configuration.GetSection(DraftsOptions.SectionName));
    services.AddScoped<ParticipantResolver>();
    services.AddScoped<DraftBoardParticipantResolver>();
    services.AddScoped<IDraftsIntegrationEventDispatcher, DraftsIntegrationEventDispatcher>();
    services.AddScoped<IDraftsDomainEventDispatcher, DraftsDomainEventDispatcher>();
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
