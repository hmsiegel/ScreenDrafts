using ScreenDrafts.Modules.Movies.Features.Movies.Shared;

namespace ScreenDrafts.Modules.Movies.Composition;

public static class MoviesModule
{
  private static readonly string _moduleName = typeof(MoviesModule).Assembly.GetName().Name!;

  public static IServiceCollection AddMoviesModule(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();

    services.AddMoviesInfrastructure(configuration);

    services.AddMoviesFeatures();

    return services;
  }

  public static IServiceCollection AddMoviesSeeding(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.AddMoviesInfrastructure(configuration);
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
      .AddConsumer<IntegrationEventConsumer<MediaFetchedIntegrationEvent>>()
      .Endpoint(x => x.InstanceId = moduleInstanceId);
  }

  private static void AddMoviesFeatures(this IServiceCollection services)
  {
    services.AddScoped<MediaPeopleAttacher>();
    services.AddScoped<IMovieTitleReader, MovieTitleReader>();
    services.AddScoped<IMoviesDomainEventDispatcher, MoviesDomainEventDispatcher>();
    services.AddScoped<IMoviesIntegrationEventDispatcher, MoviesIntegrationEventDispatcher>();
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MoviesUnitOfWorkBehavior<,>));
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
