namespace ScreenDrafts.Modules.Audit.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
  protected static readonly Assembly FeaturesAssembly = Audit.Features.AssemblyReference.Assembly;

  protected static readonly Assembly InfrastructureAssembly = typeof(AuditInfrastructure).Assembly;
}
