namespace ScreenDrafts.Web.Abstractions;
internal static class ModuleReferences
{
  private static readonly string[] _modules = [
    "Administration",
    "Audit",
    "Communications",
    "Drafts",
    "Integrations",
    "Movies",
    "RealTimeUpdates",
    "Reporting",
    "Users",
  ];

  public static string[] Modules => _modules;
}
