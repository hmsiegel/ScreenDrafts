namespace ScreenDrafts.Web.Extensions;

internal static class ConfigurationExtensions
{
  internal static void AddModuleConfiguration(
    this IConfigurationBuilder configurationBuilder,
    string[] modules,
    string environmentName
  )
  {
    foreach (var module in modules)
    {
      configurationBuilder.AddJsonFile($"modules.{module}.json", false, true);
      configurationBuilder.AddJsonFile($"modules.{module}.{environmentName}.json", true, true);
    }
  }
}
