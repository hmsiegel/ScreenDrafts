namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>
/// The movie honorific levels. Most go by the number of canonical episodes a title has been drafted on; Unified No. 1
/// and The Cycle go by where it finished. A title "joins" a level on the appearance that earns it.
/// </summary>
internal static class TitleHonorificLevels
{
  public const string MarqueeOfFame = "marquee-of-fame";
  public const string HatTrick = "hat-trick";
  public const string GrandSlam = "grand-slam";
  public const string HighFive = "high-five";
  public const string UnifiedNo1 = "unified-no-1";
  public const string TheCycle = "the-cycle";

  public static IReadOnlyList<TitleHonorificLevel> All { get; } =
  [
    new(MarqueeOfFame, "Marquee of Fame", 2),
    new(HatTrick, "Hat Trick", 3),
    new(GrandSlam, "Grand Slam", 4),
    new(HighFive, "High Five", 5),
    new("6-drafts", "6+ Drafts", 6),
    new("7-drafts", "7+ Drafts", 7),
    new("8-drafts", "8+ Drafts", 8),
    new("9-drafts", "9+ Drafts", 9),
    new("10-drafts", "10+ Drafts", 10),
    new(UnifiedNo1, "Unified No. 1", 2, TitleHonorificKind.UnifiedNo1),
    new(TheCycle, "The Cycle", 4, TitleHonorificKind.TheCycle),
  ];

  public static TitleHonorificLevel? Find(string code) =>
    All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
}
