﻿namespace ScreenDrafts.Modules.Users.ArchitectureTests.Presentation;

public class PresentationTests : BaseTest
{
  [Fact]
  public void IntegrationEventConsumer_Should_NotBePublic()
  {
    Types.InAssembly(PresentationAssembly)
        .That()
        .ImplementInterface(typeof(IIntegrationEventHandler<>))
        .Or()
        .Inherit(typeof(IntegrationEventHandler<>))
        .Should()
        .NotBePublic()
        .GetResult()
        .ShouldBeSuccessful();
  }

  [Fact]
  public void IntegrationEventConsumer_Should_BeSealed()
  {
    Types.InAssembly(PresentationAssembly)
        .That()
        .ImplementInterface(typeof(IIntegrationEventHandler<>))
        .Or()
        .Inherit(typeof(IntegrationEventHandler<>))
        .Should()
        .BeSealed()
        .GetResult()
        .ShouldBeSuccessful();
  }

  [Fact]
  public void IntegrationEventConsumer_ShouldHave_NameEndingWith_IntegrationEventConsumer()
  {
    Types.InAssembly(PresentationAssembly)
        .That()
        .ImplementInterface(typeof(IIntegrationEventHandler<>))
        .Or()
        .Inherit(typeof(IntegrationEventHandler<>))
        .Should()
        .HaveNameEndingWith("IntegrationEventConsumer", StringComparison.InvariantCultureIgnoreCase)
        .GetResult()
        .ShouldBeSuccessful();
  }
}
