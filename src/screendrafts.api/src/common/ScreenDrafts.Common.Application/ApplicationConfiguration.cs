﻿namespace ScreenDrafts.Common.Application;

public static class ApplicationConfiguration
{
  public static IServiceCollection AddApplication(this IServiceCollection services, Assembly[] moduleAssemblies)
  {

    services.AddMediatR(config =>
    {
      config.RegisterServicesFromAssemblies(moduleAssemblies);
      config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
      config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
      config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
    });

    services.AddValidatorsFromAssemblies(moduleAssemblies);

    return services;
  }
}
