namespace ScreenDrafts.Web.Abstractions;

internal static class AssemblyReferences
{
  private static readonly Assembly[] _featureAssemblies = [
    Modules.Administration.Features.AssemblyReference.Assembly,
    Modules.Audit.Features.AssemblyReference.Assembly,
    Modules.Communications.Features.AssemblyReference.Assembly,
    Modules.Drafts.Features.AssemblyReference.Assembly,
    Modules.Integrations.Features.AssemblyReference.Assembly,
    Modules.Movies.Features.AssemblyReference.Assembly,
    Modules.RealTimeUpdates.Features.AssemblyReference.Assembly,
    Modules.Reporting.Features.AssemblyReference.Assembly,
    Modules.Users.Features.AssemblyReference.Assembly
    ];

  private static readonly Assembly[] _infrastructureAssemblies = [
    typeof(AdministrationInfrastructure).Assembly,
    typeof(AuditInfrastructure).Assembly,
    typeof(CommunicationsInfrastructure).Assembly,
    typeof(DraftsInfrastructure).Assembly,
    typeof(IntegrationsInfrastructure).Assembly,
    typeof(MoviesInfrastructure).Assembly,
    typeof(RealTimeUpdatesInfrastructure).Assembly,
    typeof(ReportingInfrastructure).Assembly,
    typeof(UsersInfrastructure).Assembly,
    ];

  public static Assembly[] InfrastructureAssemblies => _infrastructureAssemblies;

  public static Assembly[] FeatureAssemblies => _featureAssemblies;
}
