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

    services.AddCommunicationsFeatures(configuration);

    services.AddCommunicationsInfrastructure(configuration);

    return services;
  }
  public static void AddCommunicationsFeatures(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
    services.AddScoped<IEmailService, SmtpEmailService>();

    services.AddScoped<ICommunicationsIntegrationEventDispatcher, CommunicationsIntegrationEventDispatcher>();
    services.AddScoped<ICommunicationsDomainEventDispatcher, CommunicationsDomainEventDispatcher>();
  }

  public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<DraftPartParticipantAddedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<DraftPartHostAddedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<UserRegisteredIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);


    registrationConfigurator.AddConsumer<IntegrationEventConsumer<DraftCompletedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<DraftCreatedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);
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
