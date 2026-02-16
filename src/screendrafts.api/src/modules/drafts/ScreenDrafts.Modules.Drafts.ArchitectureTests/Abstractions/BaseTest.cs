namespace ScreenDrafts.Modules.Drafts.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = Drafts.Features.AssemblyReference.Assembly;

  protected static readonly Assembly DomainAssembly = typeof(Draft).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(DraftsInfrastructure).Assembly;
}
