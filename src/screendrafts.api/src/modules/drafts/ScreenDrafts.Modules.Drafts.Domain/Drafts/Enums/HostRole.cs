namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class HostRole(string name, int value) : SmartEnum<HostRole>(name, value)
{
  public static readonly HostRole Primary = new("Primary", 0);
  public static readonly HostRole CoHost = new("Co-Host", 1);
}
