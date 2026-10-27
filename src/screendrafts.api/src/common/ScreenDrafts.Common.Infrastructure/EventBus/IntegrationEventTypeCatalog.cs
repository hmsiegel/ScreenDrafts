namespace ScreenDrafts.Common.Infrastructure.EventBus;

/// <summary>
/// Finds every concrete integration event in the deployed <c>*.IntegrationEvents</c> assemblies.
/// Rebus delivers by exact message type, so a module that wants every event (audit) must list them.
/// </summary>
public static class IntegrationEventTypeCatalog
{
  private const string AssemblyPattern = "ScreenDrafts.Modules.*.IntegrationEvents.dll";

  public static IReadOnlyList<Type> GetAll() =>
    [
      .. Directory
        .GetFiles(AppContext.BaseDirectory, AssemblyPattern)
        .Select(path => Assembly.Load(AssemblyName.GetAssemblyName(path)))
        .SelectMany(assembly => assembly.GetExportedTypes())
        .Where(type =>
          type is { IsClass: true, IsAbstract: false }
          && typeof(IIntegrationEvent).IsAssignableFrom(type)
        )
        .OrderBy(type => type.FullName, StringComparer.Ordinal),
    ];
}
