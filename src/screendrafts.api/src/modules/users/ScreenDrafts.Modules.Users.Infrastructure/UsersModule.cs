namespace ScreenDrafts.Modules.Users.Infrastructure;

public static class UsersModule
{
  private const string ModuleName = "Users";
  public static IServiceCollection AddUsersModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddInfrastructure(configuration);

    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddIdentity(configuration);

    services.AddDbContext<UsersDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.Users, sp);
    });

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

    services.AddScoped<IUsersApi, UsersApi>();

    services.Configure<OutboxOptions>(configuration.GetSection("Users:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Users:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();
  }

  private static void AddDomainEventHandlers(this IServiceCollection services)
  {
    Type[] domainEventHandlers = [.. Application.AssemblyReference.Assembly
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
    Type[] integrationEventHandlers = [.. Presentation.AssemblyReference.Assembly
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
