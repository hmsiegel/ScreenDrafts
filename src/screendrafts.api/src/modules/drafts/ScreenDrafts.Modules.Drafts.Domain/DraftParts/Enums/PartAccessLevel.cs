namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
public sealed class PartAccessLevel(string name, int value)
  : SmartEnum<PartAccessLevel>(name, value)
{
  public static readonly PartAccessLevel Public = new(nameof(Public), 0);
  public static readonly PartAccessLevel Patreon = new(nameof(Patreon), 1);
  public static readonly PartAccessLevel Unreleased = new(nameof(Unreleased), 2);
}
