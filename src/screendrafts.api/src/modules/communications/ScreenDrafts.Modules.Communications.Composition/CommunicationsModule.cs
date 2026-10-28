using ScreenDrafts.Common.Infrastructure.EventBus;
using ScreenDrafts.Modules.Communications.Features.ContactMessages;

namespace ScreenDrafts.Modules.Communications.Composition;

public static class CommunicationsModule
{
  private static readonly string _moduleName = typeof(CommunicationsModule)
    .Assembly.GetName()
    .Name!;

  public static IServiceCollection AddCommunicationsModule(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddCommunicationsFeatures(configuration);

    services.AddCommunicationsInfrastructure(configuration);

    return services;
  }

  public static void AddCommunicationsFeatures(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
    services.Configure<OwnerNotificationOptions>(
      configuration.GetSection(OwnerNotificationOptions.SectionName)
    );
    services.Configure<ContactOptions>(configuration.GetSection(ContactOptions.SectionName));
    services.AddScoped<IEmailService, SmtpEmailService>();

    services.AddScoped<
      ICommunicationsIntegrationEventDispatcher,
      CommunicationsIntegrationEventDispatcher
    >();
    services.AddScoped<ICommunicationsDomainEventDispatcher, CommunicationsDomainEventDispatcher>();
  }

  public static void ConfigureConsumers(
    IModuleConsumerRegistry registrationConfigurator,
    string instanceId
  )
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    var moduleInstanceId = $"{instanceId}-{_moduleName.ToLowerInvariant()}";

    registrationConfigurator.ForQueue(moduleInstanceId.ToLowerInvariant());

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DraftPartParticipantAddedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DraftPartHostAddedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<UserRegisteredIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<DraftCompletedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<DraftCreatedIntegrationEvent>>();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<UserEmailChangedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<EmailChangeConfirmationRequestedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<UserRoleAddedIntegrationEvent>>();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<UserRoleRemovedIntegrationEvent>
    >();

    registrationConfigurator.AddConsumer<
      IntegrationEventConsumer<GuestDraftInvitationSentIntegrationEvent>
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
