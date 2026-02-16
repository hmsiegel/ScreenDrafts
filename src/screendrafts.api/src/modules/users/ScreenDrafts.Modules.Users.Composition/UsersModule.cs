using ScreenDrafts.Common.Abstractions.Authorization;

namespace ScreenDrafts.Modules.Users.Composition;

public static class UsersModule
{
  public static IServiceCollection AddUsersModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddIdentity(configuration);

    services.AddUsersFeatures();

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddUsersInfratructure(configuration);

    return services;
  }

  public static IServiceCollection AddUsersSeeding(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    services.Configure<KeyCloakOptions>(configuration.GetSection("Users:KeyCloak"));

    services.AddTransient<KeyCloakAuthDelegatingHandler>();

    services
      .AddHttpClient<KeyCloakClient>((sp, client) =>
      {
        var keyCloakOptions = sp.GetRequiredService<IOptions<KeyCloakOptions>>().Value;

        Log.Information("Admin URL for KeyCloak: {AdminUrl}", keyCloakOptions.AdminUrl);

        client.BaseAddress = new Uri(keyCloakOptions.AdminUrl);

        Log.Information("KeyCloak Client Base Address: {BaseAddress}", client.BaseAddress);
      })
      .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();
    services.AddTransient<IIdentityProviderService, IdentityProviderService>();
    services.AddUsersInfratructure(configuration);
    return services;
  }


  private static IServiceCollection AddUsersFeatures(this IServiceCollection services)
  {
    services.AddScoped<IUsersApi, UsersApi>();
    services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UsersUnitOfWorkBehavior<,>));
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

  private static void AddIdentity(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IPermissionService, PermissionService>();

    services.Configure<KeyCloakOptions>(configuration.GetSection("Users:KeyCloak"));

    services.AddTransient<KeyCloakAuthDelegatingHandler>();

    services
      .AddHttpClient<KeyCloakClient>((sp, client) =>
      {
        var keyCloakOptions = sp.GetRequiredService<IOptions<KeyCloakOptions>>().Value;

        Log.Information("Admin URL for KeyCloak: {AdminUrl}", keyCloakOptions.AdminUrl);

        client.BaseAddress = new Uri(keyCloakOptions.AdminUrl);

        Log.Information("KeyCloak Client Base Address: {BaseAddress}", client.BaseAddress);
      })
      .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

    services.AddTransient<IIdentityProviderService, IdentityProviderService>();
  }
}
