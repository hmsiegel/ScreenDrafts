namespace ScreenDrafts.Common.Features.Abstractions.Services;

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
  public const string Pick = "p";
  public const string Veto = "v";
  public const string VetoOverride = "vo";
  public const string CommissionerOverride = "co";
  public const string Person = "pe";
  public const string User = "u";
}
