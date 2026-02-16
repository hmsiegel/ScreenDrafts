namespace ScreenDrafts.Modules.Users.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = Users.Features.AssemblyReference.Assembly;

  protected static readonly Assembly DomainAssembly = typeof(User).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(UsersInfrastructure).Assembly;
}
