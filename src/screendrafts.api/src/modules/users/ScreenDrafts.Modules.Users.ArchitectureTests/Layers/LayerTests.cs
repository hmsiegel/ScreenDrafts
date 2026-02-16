namespace ScreenDrafts.Modules.Users.ArchitectureTests.Layers;

public class LayerTests : BaseTest
{
  [Fact]
  public void DomainLayer_ShouldNotHaveDependencyOn_ApplicationLayer()
  {
    Types.InAssembly(DomainAssembly)
        .Should()
        .NotHaveDependencyOn(FeaturesAssembly.GetName().Name)
        .GetResult()
        .ShouldBeSuccessful();
  }

  [Fact]
  public void DomainLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
  {
    Types
        .InAssembly(DomainAssembly)
        .Should()
        .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
        .GetResult()
        .ShouldBeSuccessful();
  }
}
