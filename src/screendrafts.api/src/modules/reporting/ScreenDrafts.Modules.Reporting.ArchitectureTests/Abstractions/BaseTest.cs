namespace ScreenDrafts.Modules.Reporting.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = Reporting.Features.AssemblyReference.Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(ReportingInfrastructure).Assembly;
}
