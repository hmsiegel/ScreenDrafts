﻿namespace ScreenDrafts.Modules.Integrations.Infrastructure;

public static class IntegrationsModule
{
  public static IServiceCollection AddIntegrationsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddInfrastructure(configuration);

    return services;
  }

  public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
  {
    ArgumentNullException.ThrowIfNull(registrationConfigurator);

    registrationConfigurator.AddConsumer<IntegrationEventConsumer<FetchMovieRequestedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = instanceId);
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Database")!;

    services.AddDbContext<IntegrationsDbContext>((sp, options) =>
      options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Integrations))
      .UseSnakeCaseNamingConvention()
      .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IntegrationsDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Integrations:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Integrations:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    services.AddOptions<ImdbSettings>()
      .Bind(configuration.GetSection("Integrations:Imdb"))
      .ValidateDataAnnotations();

    services.AddScoped<IImdbService, ImdbService>();

    services.AddOptions<OmdbSettings>()
      .Bind(configuration.GetSection("Integrations:Omdb"))
      .ValidateDataAnnotations();

    services.AddScoped<IOmdbService, OmdbService>();
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
