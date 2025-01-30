namespace ScreenDrafts.Web.Abstractions;
internal static class AssemblyReferences
{
  private static readonly Assembly[] _presentationAssemblies = [
    Modules.Administration.Presentation.AssemblyReference.Assembly,
  Modules.Audit.Presentation.AssemblyReference.Assembly,
  Modules.Communications.Presentation.AssemblyReference.Assembly,
  Modules.Drafts.Presentation.AssemblyReference.Assembly,
  Modules.Integrations.Presentation.AssemblyReference.Assembly,
  Modules.Movies.Presentation.AssemblyReference.Assembly,
  Modules.RealTimeUpdates.Presentation.AssemblyReference.Assembly,
  Modules.Reporting.Presentation.AssemblyReference.Assembly,
  Modules.Users.Presentation.AssemblyReference.Assembly,
  ];

  private static readonly Assembly[] _applicationAssemblies = [
    Modules.Administration.Application.AssemblyReference.Assembly,
  Modules.Audit.Application.AssemblyReference.Assembly,
  Modules.Communications.Application.AssemblyReference.Assembly,
  Modules.Drafts.Application.AssemblyReference.Assembly,
  Modules.Integrations.Application.AssemblyReference.Assembly,
  Modules.Movies.Application.AssemblyReference.Assembly,
  Modules.RealTimeUpdates.Application.AssemblyReference.Assembly,
  Modules.Reporting.Application.AssemblyReference.Assembly,
  Modules.Users.Application.AssemblyReference.Assembly
    ];

  public static Assembly[] ApplicationAssemblies => _applicationAssemblies;

  public static Assembly[] PresentationAssemblies => _presentationAssemblies;
}
