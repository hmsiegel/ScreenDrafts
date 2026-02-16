namespace ScreenDrafts.Modules.Audit.ArchitectureTests.Layers;

public class LayerTests : BaseTest
{
  [Fact]
  public void FeatureLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
  {
    Types.InAssembly(FeaturesAssembly)
        .Should()
        .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
        .GetResult()
        .ShouldBeSuccessful();
  }
}
