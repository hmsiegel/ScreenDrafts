namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;

public sealed class CommunityFilmRuleKind(string name, int value)
  : SmartEnum<CommunityFilmRuleKind>(name, value)
{
  public static readonly CommunityFilmRuleKind BoostersVeto = new(nameof(BoostersVeto), 0);
  public static readonly CommunityFilmRuleKind BoostersPick = new(nameof(BoostersPick), 1);
  public static readonly CommunityFilmRuleKind PhantomTopN = new(nameof(PhantomTopN), 2);
  public static readonly CommunityFilmRuleKind PhantomExactSlot = new(nameof(PhantomExactSlot), 3);
}
