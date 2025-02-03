namespace ScreenDrafts.Modules.Reporting.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly ApplicationAssembly = typeof(Reporting.Application.AssemblyReference).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(ReportingModule).Assembly;

  protected static readonly Assembly PresentationAssembly = typeof(Reporting.Presentation.AssemblyReference).Assembly;
}
