namespace ScreenDrafts.Modules.GuestDrafts.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = AssemblyReference.Assembly;

  protected static readonly Assembly DomainAssembly = typeof(GuestDraft).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(GuestDraftsInfrastructure).Assembly;
}
