using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts;

namespace ScreenDrafts.Modules.GuestDrafts.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = AssemblyReference.Assembly;

  protected static readonly Assembly DomainAssembly = typeof(Draft).Assembly;

  protected static readonly Assembly InfrastructureAssembly =
    typeof(GuestDraftsInfrastructure).Assembly;
}
