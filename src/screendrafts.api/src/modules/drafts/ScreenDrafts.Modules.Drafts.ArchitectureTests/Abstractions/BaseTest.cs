namespace ScreenDrafts.Modules.Drafts.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly ApplicationAssembly = typeof(Drafts.Application.AssemblyReference).Assembly;

  protected static readonly Assembly DomainAssembly = typeof(Draft).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(DraftsModule).Assembly;

  protected static readonly Assembly PresentationAssembly = typeof(Drafts.Presentation.AssemblyReference).Assembly;
}
