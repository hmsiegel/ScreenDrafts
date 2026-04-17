using ScreenDrafts.Modules.Audit.Features.Common;

namespace ScreenDrafts.Modules.Audit.Composition;

public static class AuditModule
{
  public static IServiceCollection AddAuditModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddAuditInfrastructure(configuration);

    services.AddAuditFeatures(configuration);

    return services;
  }
  public static void AddAuditFeatures(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddScoped<IAuditIntegrationEventDispatcher, AuditIntegrationEventDispatcher>();
    services.AddScoped<IAuditDomainEventDispatcher, AuditDomainEventDispatcher>();
    services.AddScoped(typeof(ExportHelpers<>));
  }

  public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);
    registrationConfigurator.AddConsumer<GenericIntegrationAuditConsumer>()
      .Endpoint(c => c.InstanceId = instanceId);
  }

  public static void ConfigureEndpoints(IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
  {
    ArgumentNullException.ThrowIfNull(configurator);

    configurator.ReceiveEndpoint("screendrafts.audit", endpoint =>
    {
      endpoint.ConfigureConsumeTopology = false;

      endpoint.Bind("ScreenDrafts.Common.Application.EventBus:IIntegrationEvent", x =>
      {
        x.ExchangeType = "fanout";
        x.Durable = true;
        x.AutoDelete = false;
      });

      endpoint.ConfigureConsumer<GenericIntegrationAuditConsumer>(context);
    });
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
