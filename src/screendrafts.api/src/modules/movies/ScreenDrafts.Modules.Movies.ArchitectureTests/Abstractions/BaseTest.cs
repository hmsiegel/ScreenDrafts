namespace ScreenDrafts.Modules.Movies.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = Movies.Features.AssemblyReference.Assembly;

  protected static readonly Assembly DomainAssembly = typeof(Media).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(MoviesInfrastructure).Assembly;

}
