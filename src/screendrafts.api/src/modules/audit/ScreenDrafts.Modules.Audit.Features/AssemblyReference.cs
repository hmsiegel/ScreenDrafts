using System.Reflection;

namespace ScreenDrafts.Modules.Audit.Features;

public static class AssemblyReference
{
  public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
