namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Enums;

public sealed class VetoIssuerKind(string name, int value)
  : SmartEnum<VetoIssuerKind>(name, value)
{
  public static readonly VetoIssuerKind Participant = new(nameof(Participant), 0);
  public static readonly VetoIssuerKind Patreon = new(nameof(Patreon), 1);
  public static readonly VetoIssuerKind Other = new(nameof(Other), 2);
}
