using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ScreenDrafts.Common.Application.EventBus;
using ScreenDrafts.Common.Application.Messaging;
using ScreenDrafts.Modules.Administration.Features;
using ScreenDrafts.Modules.Administration.Features.Inbox;
using ScreenDrafts.Modules.Administration.Features.Outbox;
using ScreenDrafts.Modules.Administration.Infrastructure;
using ScreenDrafts.Modules.Administration.Infrastructure.Inbox;
using ScreenDrafts.Modules.Administration.Infrastructure.Outbox;

namespace ScreenDrafts.Modules.Administration.Composition;

public static class AdministrationModule
{

  public static IServiceCollection AddAdministrationModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddAdministrationInfrastructure(configuration);

    services.AddAdministrationFeatures();

    return services;
  }
  public static void AddAdministrationFeatures(this IServiceCollection services)
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
