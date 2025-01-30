namespace ScreenDrafts.Web.Extensions;
internal static class ModuleServiceExtensions
{
  public static void AddModules(IServiceCollection services, IConfiguration configuration)
  {
    services.AddAdministrationModule(configuration);
    services.AddAuditModule(configuration);
    services.AddCommunicationsModule(configuration);
    services.AddDraftsModule(configuration);
    services.AddIntegrationsModule(configuration);
    services.AddMoviesModule(configuration);
    services.AddRealTimeUpdatesModule(configuration);
    services.AddReportingModule(configuration);
    services.AddUsersModule(configuration);
  }
}
