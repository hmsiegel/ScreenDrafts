namespace ScreenDrafts.Modules.Drafts.Infrastructure;

public static class DraftsModule
{
  private const string ModuleName = "Drafts";
  public static IServiceCollection AddDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDraftsApplication();

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddInfrastructure(configuration);

    return services;
  }

  public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<UserRegisteredIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<MovieAddedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<DraftsDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.Drafts, sp);
    });

    SqlMapper.AddTypeHandler(new DraftPositionsTypeHandler());

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DraftsDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Drafts:Outbox"));
    services.ConfigureOptions<ConfigureProcessOutboxJob>();
    services.Configure<InboxOptions>(configuration.GetSection("Drafts:Inbox"));
    services.ConfigureOptions<ConfigureProcessInboxJob>();
  }

  private static IServiceCollection AddDraftsApplication(this IServiceCollection services)
  {
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DraftsUnitOfWorkBehavior<,>));
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
