namespace ScreenDrafts.Modules.RealTimeUpdates.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly ApplicationAssembly = typeof(RealTimeUpdates.Application.AssemblyReference).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(RealTimeUpdatesModule).Assembly;

  protected static readonly Assembly PresentationAssembly = typeof(RealTimeUpdates.Presentation.AssemblyReference).Assembly;
}
