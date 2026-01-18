namespace ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
public static class SeedingHelper
{
  public static HashSet<string> ParseModules(string[] args, bool allowAll = true)
  {
    ArgumentNullException.ThrowIfNull(args);

    var selectedModules = new HashSet<string>();

    foreach (var arg in args.Where(arg => arg.StartsWith("--module=", StringComparison.OrdinalIgnoreCase)))
    {
      var parts = arg.Split('=');
      if (parts.Length == 2)
      {
        foreach (var module in parts[1]
          .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
          .Where(module => allowAll || !string.Equals(module, "all", StringComparison.OrdinalIgnoreCase)))
        {
          selectedModules.Add(module.ToLowerInvariant());
        }
      }
    }

    if (selectedModules.Count == 0)
    {
      selectedModules.Add("all");
    }

    return selectedModules;
  }

  public static IEnumerable<TSeeder> FilterAndOrderSeeders<TSeeder>(
    IEnumerable<TSeeder> seeders,
    HashSet<string> selectedModules)
    where TSeeder : ICustomSeeder
  {
    ArgumentNullException.ThrowIfNull(selectedModules);

    if (selectedModules.Contains("all"))
    {
      return seeders.OrderBy(s => s.Order);
    }

    return seeders
      .Where(s => selectedModules.Contains(s.Name.ToLowerInvariant()))
      .OrderBy(s => s.Order);
  }
}
