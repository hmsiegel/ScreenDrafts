using ScreenDrafts.Modules.Drafts.Infrastructure.Database.DatabaseSeeders;

namespace ScreenDrafts.Modules.Drafts.Infrastructure;

public static class DraftsModule
{
  public static IServiceCollection AddDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddInfrastructure(configuration);

    return services;
  }

  public static async Task<IApplicationBuilder> UseDraftsModuleAsync(this IApplicationBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    await app.UseSeedersAsync();

    return app;
  }

  public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<UserRegisteredIntegrationEvent>>()
      .Endpoint(c => c.InstanceId = instanceId);
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Database")!;

    services.AddDbContext<DraftsDbContext>((sp, options) =>
      options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Drafts))
      .UseSnakeCaseNamingConvention()
      .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DraftsDbContext>());

    services.RegisterRepositories();

    services.AddScoped<ICustomSeeder, DrafterSeeder>();
    services.AddScoped<ICustomSeeder, HostsSeeder>();
    services.AddScoped<ICustomSeeder, DraftSeeder>();

    services.Configure<OutboxOptions>(configuration.GetSection("Drafts:Outbox"));
    services.ConfigureOptions<ConfigureProcessOutboxJob>();
    services.Configure<InboxOptions>(configuration.GetSection("Drafts:Inbox"));
    services.ConfigureOptions<ConfigureProcessInboxJob>();
  }

  private static void RegisterRepositories(this IServiceCollection services)
  {
    services.AddScoped<IDraftsRepository, DraftsRepository>();
    services.AddScoped<IDraftersRepository, DraftersRepository>();
    services.AddScoped<IGameBoardRepository, GameBoardRepository>();
    services.AddScoped<IHostsRepository, HostsRepository>();
    services.AddScoped<IPicksRepository, PicksRepository>();
    services.AddScoped<IVetoRepository, VetoRepository>();
    services.AddScoped<ITriviaResultsRepository, TriviaResultsRepository>();
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

}
