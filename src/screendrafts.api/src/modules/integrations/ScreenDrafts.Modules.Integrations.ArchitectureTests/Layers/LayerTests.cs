namespace ScreenDrafts.Modules.Integrations.ArchitectureTests.Layers;

public class LayerTests : BaseTest
{
  [Fact]
  public void FeaturesLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
  {
    Types.InAssembly(FeaturesAssembly)
        .Should()
        .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
        .GetResult()
        .ShouldBeSuccessful();
  }
}
