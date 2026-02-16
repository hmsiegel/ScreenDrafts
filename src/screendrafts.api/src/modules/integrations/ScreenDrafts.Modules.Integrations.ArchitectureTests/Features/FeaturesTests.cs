namespace ScreenDrafts.Modules.Integrations.ArchitectureTests.Features;

public class FeaturesTests : BaseTest
{
  [Fact]
  public void Command_Should_BeSealed()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(ICommand))
      .Or()
      .ImplementInterface(typeof(ICommand<>))
      .Should()
      .BeSealed()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void Command_ShouldHave_NameEndingWith_Command()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(ICommand))
      .Or()
      .ImplementInterface(typeof(ICommand<>))
      .Should()
      .HaveNameEndingWith("Command", StringComparison.InvariantCulture)
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void CommandHandler_Should_NotBePublic()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(ICommandHandler<>))
      .Or()
      .ImplementInterface(typeof(ICommandHandler<,>))
      .Should()
      .NotBePublic()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void CommandHandler_ShouldHave_NameEndingWith_CommandHandler()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(ICommandHandler<>))
      .Or()
      .ImplementInterface(typeof(ICommandHandler<,>))
      .Should()
      .HaveNameEndingWith("CommandHandler", StringComparison.InvariantCulture)
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void CommandHandler_Should_BeSealed()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(ICommandHandler<>))
      .Or()
      .ImplementInterface(typeof(ICommandHandler<,>))
      .Should()
      .BeSealed()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void Query_Should_BeSealed()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(IQuery<>))
      .Should()
      .BeSealed()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void Query_ShouldHave_NameEndingWith_Query()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(IQuery<>))
      .Should()
      .HaveNameEndingWith("Query", StringComparison.InvariantCulture)
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void QueryHandler_Should_NotBePublic()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(IQueryHandler<,>))
      .Should()
      .NotBePublic()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void QueryHandler_ShouldHave_NameEndingWith_QueryHandler()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(IQueryHandler<,>))
      .Should()
      .HaveNameEndingWith("QueryHandler", StringComparison.InvariantCulture)
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void Validator_Should_NotBePublic()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .Inherit(typeof(AbstractValidator<>))
      .Should()
      .NotBePublic()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void Validator_ShouldHave_NameEndingWith_Validator()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .Inherit(typeof(AbstractValidator<>))
      .Should()
      .HaveNameEndingWith("Validator", StringComparison.InvariantCulture)
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void Validator_Should_BeSealed()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .Inherit(typeof(AbstractValidator<>))
      .Should()
      .BeSealed()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void DomainEventHandler_Should_NotBePublic()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(IDomainEventHandler<>))
      .Or()
      .Inherit(typeof(DomainEventHandler<>))
      .Should()
      .NotBePublic()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void DomainEventHandler_ShouldHave_NameEndingWith_DomainEventHandler()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(IDomainEventHandler<>))
      .Or()
      .Inherit(typeof(DomainEventHandler<>))
      .Should()
      .HaveNameEndingWith("DomainEventHandler", StringComparison.InvariantCulture)
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void DomainEventHandler_Should_BeSealed()
  {
    Types.InAssembly(FeaturesAssembly)
      .That()
      .ImplementInterface(typeof(IDomainEventHandler<>))
      .Or()
      .Inherit(typeof(DomainEventHandler<>))
      .Should()
      .BeSealed()
      .GetResult()
      .ShouldBeSuccessful();
  }

  [Fact]
  public void IntegrationEventConsumer_Should_NotBePublic()
  {
    Types.InAssembly(FeaturesAssembly)
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
    Types.InAssembly(FeaturesAssembly)
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
    Types.InAssembly(FeaturesAssembly)
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
