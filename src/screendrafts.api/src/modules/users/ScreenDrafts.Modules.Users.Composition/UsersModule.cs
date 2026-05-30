using ScreenDrafts.Modules.Drafts.IntegrationEvents;

namespace ScreenDrafts.Modules.Users.Composition;

public static class UsersModule
{
  private static readonly string _moduleName = typeof(UsersModule).Assembly.GetName().Name!;

  public static IServiceCollection AddUsersModule(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddIdentity(configuration);

    services.AddUsersFeatures();

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddUsersInfratructure(configuration);

    return services;
  }

  public static IServiceCollection AddUsersSeeding(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);
    services.AddKeyCloakClient(configuration);
    services.AddTransient<IIdentityProviderService, IdentityProviderService>();
    services.AddUsersInfratructure(configuration);
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
      .AddConsumer<IntegrationEventConsumer<PermissionAddedToRoleIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);
    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<UserRoleAddedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);
    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<UserRoleRemovedIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);
    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<PermissionRemovedFromRoleIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);
    registrationConfigurator
      .AddConsumer<IntegrationEventConsumer<PersonCreatedForUserIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = moduleInstanceId);
  }

  private static IServiceCollection AddUsersFeatures(this IServiceCollection services)
  {
    services.AddScoped<IUsersApi, UsersApi>();
    services.AddScoped<IUsersDomainEventDispatcher, UsersDomainEventDispatcher>();
    services.AddScoped<IUsersIntegrationEventDispatcher, UsersIntegrationEventDispatcher>();
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UsersUnitOfWorkBehavior<,>));
    return services;
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

  private static void AddIdentity(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IPermissionService, PermissionService>();

    services.AddKeyCloakClient(configuration);

    services.AddTransient<IIdentityProviderService, IdentityProviderService>();
  }
}
