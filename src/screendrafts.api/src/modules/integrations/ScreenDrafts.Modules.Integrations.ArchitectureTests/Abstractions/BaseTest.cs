namespace ScreenDrafts.Modules.Integrations.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly ApplicationAssembly = typeof(Integrations.Application.AssemblyReference).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(IntegrationsModule).Assembly;

  protected static readonly Assembly PresentationAssembly = typeof(Integrations.Presentation.AssemblyReference).Assembly;
}
