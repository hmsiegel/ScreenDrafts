namespace ScreenDrafts.Web.Abstractions;
internal static class ModuleReferences
{
  private static readonly string[] _modules = [
    "administration",
    "audit",
    "communications",
    "drafts",
    "integrations",
    "movies",
    "realtimeupdates",
    "reporting",
    "users",
  ];

  public static string[] Modules => _modules;
}
