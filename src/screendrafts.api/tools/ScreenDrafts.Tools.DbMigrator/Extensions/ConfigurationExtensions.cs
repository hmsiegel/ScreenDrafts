namespace ScreenDrafts.Tools.DbMigrator.Extensions;

public static class ConfigurationExtensions
{
  public static string GetConnectionStringOrThrow(this IConfiguration configuration, string name)
  {
    return configuration.GetConnectionString(name) ??
      throw new InvalidOperationException($"Connection string '{name}' not found.");
  }

  public static T GetValueOrThrow<T>(this IConfiguration configuration, string name)
  {
    return configuration.GetValue<T>(name) ??
      throw new InvalidOperationException($"Configuration value '{name}' not found.");
  }
}
