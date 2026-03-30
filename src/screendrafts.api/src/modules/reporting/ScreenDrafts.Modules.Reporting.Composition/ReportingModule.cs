using MassTransit;

using ScreenDrafts.Modules.Drafts.IntegrationEvents;

namespace ScreenDrafts.Modules.Reporting.Composition;

public static class ReportingModule
{

  public static IServiceCollection AddReportingModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddReportingInfratsructure(configuration);

    services.AddReportingFeatures();

    return services;
  }

  private static void AddReportingFeatures(this IServiceCollection services)
  {
    services.AddScoped<IReportingIntegrationEventDispatcher, ReportingIntegrationEventDispatcher>();
    services.AddScoped<IReportingDomainEventDispatcher, ReportingDomainEventDispatcher>();
  }

  public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<DraftPartStartedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<PickLockedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);
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
