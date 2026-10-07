namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal static class StatsGroupBys
{
  public const string Drafter = "drafter";
  public const string Draft = "draft";
  public const string Series = "series";
  public const string DraftType = "draftType";
  public const string Title = "title";

  public static IReadOnlyList<StatsGroupByDefinition> All { get; } =
  [
    new(Drafter, "Drafter"),
    new(Draft, "Draft"),
    new(Series, "Series"),
    new(DraftType, "Draft type"),
    new(Title, "Title"),
  ];

  public static bool IsKnown(string code) => All.Any(g => g.Code == code);
}
