using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ScreenDrafts.Common.Application.EventBus;
using ScreenDrafts.Common.Application.Messaging;
using ScreenDrafts.Modules.Communications.Features;
using ScreenDrafts.Modules.Communications.Features.Inbox;
using ScreenDrafts.Modules.Communications.Features.Outbox;
using ScreenDrafts.Modules.Communications.Infrastructure;
using ScreenDrafts.Modules.Communications.Infrastructure.Inbox;
using ScreenDrafts.Modules.Communications.Infrastructure.Outbox;

namespace ScreenDrafts.Modules.Communications.Composition;

public static class CommunicationsModule
{

  public static IServiceCollection AddCommunicationsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddCommunicationsFeatures();

    services.AddCommunicationsInfrastructure(configuration);

    return services;
  }
  public static void AddCommunicationsFeatures(this IServiceCollection services)
  {
    services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
    services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
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
