namespace ScreenDrafts.Common.Presentation;

public static class PresentationConfiguration
{
  public static IServiceCollection AddPresentation(this IServiceCollection services)
  {
    services.AddAuthorizationInternal();

    return services;
  }
}
