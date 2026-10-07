namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

/// <summary>
/// The metrics the custom query supports. Each metric lists the group-bys it makes sense for, and the
/// request is rejected for any other pairing. Metrics are computed from the Record Book fact tables.
/// </summary>
internal static class StatsMetrics
{
  public const string Appearances = "appearances";
  public const string TitlesDrafted = "titlesDrafted";
  public const string PicksVetoed = "picksVetoed";
  public const string VetoesUsed = "vetoesUsed";
  public const string VetoesOverridden = "vetoesOverridden";
  public const string SelfVetoes = "selfVetoes";
  public const string CommissionerOverrides = "commissionerOverrides";
  public const string CopaceticDrafts = "copaceticDrafts";
  public const string AvgVetoesPerDraft = "avgVetoesPerDraft";

  private static readonly string[] _everything =
  [
    StatsGroupBys.Drafter,
    StatsGroupBys.Draft,
    StatsGroupBys.Series,
    StatsGroupBys.DraftType,
    StatsGroupBys.Title,
  ];

  private static readonly string[] _drafterSeriesType =
  [
    StatsGroupBys.Drafter,
    StatsGroupBys.Series,
    StatsGroupBys.DraftType,
  ];

  public static IReadOnlyList<StatsMetricDefinition> All { get; } =
  [
    new(
      Appearances,
      "Appearances",
      "count",
      "Drafts a drafter appeared in, a multi-part draft counting once. For a series or draft type: the total of drafter appearances.",
      _drafterSeriesType),
    new(
      TitlesDrafted,
      "Titles drafted",
      "count",
      "Picks that landed: not vetoed with the veto standing and not removed by the commissioner.",
      _everything),
    new(
      PicksVetoed,
      "Picks vetoed",
      "count",
      "Picks vetoed with the veto standing.",
      _everything),
    new(
      VetoesUsed,
      "Vetoes used",
      "count",
      "Vetoes deployed, overridden ones included. By drafter: vetoes that drafter issued (team vetoes are not attributed).",
      _everything),
    new(
      VetoesOverridden,
      "Vetoes overridden",
      "count",
      "Vetoes that were overridden. By drafter: vetoes that drafter issued which were overridden.",
      _everything),
    new(
      SelfVetoes,
      "Self-vetoes",
      "count",
      "Vetoes a drafter used on their own pick, with the veto standing.",
      _everything),
    new(
      CommissionerOverrides,
      "Commissioner overrides",
      "count",
      "Picks removed by commissioner override.",
      _everything),
    new(
      CopaceticDrafts,
      "Copacetic drafts",
      "count",
      "Drafts with no veto of any kind and no commissioner override. By drafter: copacetic drafts they appeared in.",
      _drafterSeriesType),
    new(
      AvgVetoesPerDraft,
      "Vetoes per draft",
      "ratio",
      "By drafter: vetoes they issued divided by their appearances. By series or draft type: all vetoes divided by drafts.",
      _drafterSeriesType),
  ];

  public static StatsMetricDefinition? Find(string code) => All.FirstOrDefault(m => m.Code == code);
}
