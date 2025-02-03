namespace ScreenDrafts.Modules.Communications.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly ApplicationAssembly = typeof(Communications.Application.AssemblyReference).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(CommunicationsModule).Assembly;

  protected static readonly Assembly PresentationAssembly = typeof(Communications.Presentation.AssemblyReference).Assembly;
}
