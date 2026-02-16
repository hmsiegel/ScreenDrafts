namespace ScreenDrafts.Modules.Administration.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = Administration.Features.AssemblyReference.Assembly;
  protected static readonly Assembly InfrastructureAssembly = typeof(AdministrationInfrastructure).Assembly;
}
