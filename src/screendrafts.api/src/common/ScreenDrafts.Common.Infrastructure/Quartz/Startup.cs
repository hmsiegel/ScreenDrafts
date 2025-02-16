namespace ScreenDrafts.Common.Infrastructure.Quartz;

internal static class Startup
{
  internal static IServiceCollection AddQuartzService(this IServiceCollection services)
  {
    services.AddQuartz();

    services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

    return services;
  }
}
