namespace ScreenDrafts.Modules.Audit.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly ApplicationAssembly = typeof(Audit.Application.AssemblyReference).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(AuditModule).Assembly;

  protected static readonly Assembly PresentationAssembly = typeof(Audit.Presentation.AssemblyReference).Assembly;
}
