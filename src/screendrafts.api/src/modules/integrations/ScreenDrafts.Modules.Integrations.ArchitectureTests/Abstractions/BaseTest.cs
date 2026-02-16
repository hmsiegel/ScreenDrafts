namespace ScreenDrafts.Modules.Integrations.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = Integrations.Features.AssemblyReference.Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(IntegrationsInfrastructure).Assembly;

}
