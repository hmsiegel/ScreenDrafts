namespace ScreenDrafts.Modules.Administration.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly ApplicationAssembly = typeof(Administration.Application.AssemblyReference).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(AdministrationModule).Assembly;

  protected static readonly Assembly PresentationAssembly = typeof(Administration.Presentation.AssemblyReference).Assembly;
}
