namespace ScreenDrafts.Common.Application.Services;

public static class PublicIdPrefixes
{
  public const int MaxPublicIdLength = 3 + 1 + 15; // prefix + '_' + nanoid
  public const string Draft = "d";
  public const string Campaign = "c";
  public const string Category = "cat";
  public const string Series = "s";
  public const string Drafter = "dr";
  public const string DrafterTeam = "dt";
  public const string Host = "h";
  public const string DraftPart = "dp";
  public const string DraftPosition = "pos";
  public const string Pick = "p";
  public const string Veto = "v";
  public const string VetoOverride = "vo";
  public const string CommissionerOverride = "co";
  public const string Person = "pe";
  public const string User = "u";
  public const string DraftBoard = "db";
  public const string DraftPool = "dpo";
  public const string Media = "m";
  public const string TriviaResult = "tr";
  public const string SubDraft = "sd";
  public const string PredictionSeason = "ps";
  public const string DraftPredictionSet = "set";
  public const string PredictionContestant = "pc";
  public const string DraftPartPredictionRules = "pr";
  public const string PredictionResult = "res";
  public const string PredictionStanding = "st";
  public const string Recording = "rec";
}
