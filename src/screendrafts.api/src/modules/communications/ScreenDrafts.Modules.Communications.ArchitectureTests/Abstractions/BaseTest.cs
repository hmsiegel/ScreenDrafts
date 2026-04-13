namespace ScreenDrafts.Modules.Communications.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = Communications.Features.AssemblyReference.Assembly;

  protected static readonly Assembly DomainAssembly = typeof(EmailMessage).Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(CommunicationsInfrastructure).Assembly;
}
