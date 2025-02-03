namespace ScreenDrafts.Modules.Audit.Infrastructure;

public static class AuditModule
{
  public static IServiceCollection AddAuditModule(
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
    var connectionString = configuration.GetConnectionString("Database")!;

    services.AddDbContext<AuditDbContext>((sp, options) =>
      options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Audit))
      .UseSnakeCaseNamingConvention()
      .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AuditDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Audit:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Audit:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();
  }
  private static void AddDomainEventHandlers(this IServiceCollection services)
  {
    Type[] domainEventHandlers = Application.AssemblyReference.Assembly
        .GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
        .ToArray();

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
    Type[] integrationEventHandlers = Presentation.AssemblyReference.Assembly
        .GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
        .ToArray();

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
