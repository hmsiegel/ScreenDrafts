namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Enums;

public sealed class VetoOverrideIssuerKind(
  string name,
  int value) : SmartEnum<VetoOverrideIssuerKind>(name, value)
{
  public static readonly VetoOverrideIssuerKind Participant = new(nameof(Participant), 0);
  public static readonly VetoOverrideIssuerKind Patreon = new(nameof(Patreon), 1);
  public static readonly VetoOverrideIssuerKind Other = new(nameof(Other), 2);
}
