namespace ScreenDrafts.Web.Abstractions;

internal static class AssemblyReferences
{
  private static readonly Assembly[] _featureAssemblies = [
    Modules.Drafts.Features.AssemblyReference.Assembly,
    Modules.Users.Features.AssemblyReference.Assembly,
    ];

  private static readonly Assembly[] _presentationAssemblies = [
    Modules.Administration.Presentation.AssemblyReference.Assembly,
  Modules.Audit.Presentation.AssemblyReference.Assembly,
  Modules.Communications.Presentation.AssemblyReference.Assembly,
  Modules.Drafts.Features.AssemblyReference.Assembly,
  Modules.Integrations.Presentation.AssemblyReference.Assembly,
  Modules.Movies.Presentation.AssemblyReference.Assembly,
  Modules.RealTimeUpdates.Presentation.AssemblyReference.Assembly,
  Modules.Reporting.Presentation.AssemblyReference.Assembly,
  ];

  private static readonly Assembly[] _applicationAssemblies = [
    Modules.Administration.Application.AssemblyReference.Assembly,
  Modules.Audit.Application.AssemblyReference.Assembly,
  Modules.Communications.Application.AssemblyReference.Assembly,
  Modules.Drafts.Features.AssemblyReference.Assembly,
  Modules.Integrations.Application.AssemblyReference.Assembly,
  Modules.Movies.Application.AssemblyReference.Assembly,
  Modules.RealTimeUpdates.Application.AssemblyReference.Assembly,
  Modules.Reporting.Application.AssemblyReference.Assembly,
    ];

  private static readonly Assembly[] _infrastructureAssemblies = [
    typeof(AdministrationModule).Assembly,
    typeof(AuditModule).Assembly,
    typeof(CommunicationsModule).Assembly,
    typeof(DraftsModule).Assembly,
    typeof(IntegrationsModule).Assembly,
    typeof(MoviesModule).Assembly,
    typeof(RealTimeUpdatesModule).Assembly,
    typeof(ReportingModule).Assembly,
    typeof(UsersModule).Assembly,
    ];

  public static Assembly[] ApplicationAssemblies => _applicationAssemblies;

  public static Assembly[] PresentationAssemblies => _presentationAssemblies;

  public static Assembly[] InfrastructureAssemblies => _infrastructureAssemblies;

  public static Assembly[] FeatureAssemblies => _featureAssemblies;
}
