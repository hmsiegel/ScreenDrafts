using ScreenDrafts.Common.Infrastructure.EventBus;
using ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

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
    IModuleConsumerRegistry registrationConfigurator,
    string instanceId
  )
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    var moduleInstanceId = $"{instanceId}-{_moduleName.ToLowerInvariant()}";

    registrationConfigurator.ForQueue(moduleInstanceId.ToLowerInvariant());

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<PickAddedIntegrationEvent>>();

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<PickRevealedIntegrationEvent>>();

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<PickSubmittedIntegrationEvent>>();

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<VetoAppliedIntegrationEvent>>();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<VetoOverrideAppliedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<CommissionerOverrideAppliedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DraftPositionAssignedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DraftPositionUnassignedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DrafterHonorificEarnedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<MovieHonorificEarnedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<VetoUndoneIntegrationEvent>>();

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<PositionsSetIntegrationEvent>>();

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<PickUndoneIntegrationEvent>>();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DraftCompletedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DraftPartCompletedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<CommunityRuleAppliedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DraftPartStartedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<SubDraftUpdatedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftCommissionerOverrideAppliedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftPickRevealedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftPickSubmittedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftPickUndoneIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftVetoAppliedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftVetoOverrideAppliedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftVetoUndoneIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftCompletedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftStartedIntegrationEvent>
    >();
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
