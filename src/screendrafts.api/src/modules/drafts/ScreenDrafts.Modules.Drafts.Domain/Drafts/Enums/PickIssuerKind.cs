namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class PickIssuerKind(string name, int value) : SmartEnum<PickIssuerKind>(name, value)
{
  public static readonly PickIssuerKind Participant = new(nameof(Participant), 0);
  public static readonly PickIssuerKind Patreon = new(nameof(Patreon), 1);
  public static readonly PickIssuerKind Other = new(nameof(Other), 2);
}
