namespace ScreenDrafts.Modules.Movies.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly ApplicationAssembly = typeof(Movies.Application.AssemblyReference).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(MoviesModule).Assembly;

  protected static readonly Assembly PresentationAssembly = typeof(Movies.Presentation.AssemblyReference).Assembly;
}
