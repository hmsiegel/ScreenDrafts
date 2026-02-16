namespace ScreenDrafts.Modules.RealTimeUpdates.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = RealTimeUpdates.Features.AssemblyReference.Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(RealTimeUpdatesInfrastructure).Assembly;
}
