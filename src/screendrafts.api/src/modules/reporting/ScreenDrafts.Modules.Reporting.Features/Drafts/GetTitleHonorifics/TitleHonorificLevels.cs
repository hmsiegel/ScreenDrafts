namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>
/// The movie honorific levels, by the number of canonical episodes a title has been drafted on.
/// A title "joins" a level on the appearance that reaches its minimum.
/// </summary>
internal static class TitleHonorificLevels
{
  public const string MarqueeOfFame = "marquee-of-fame";
  public const string HatTrick = "hat-trick";
  public const string GrandSlam = "grand-slam";
  public const string HighFive = "high-five";

  public static IReadOnlyList<TitleHonorificLevel> All { get; } =
  [
    new(MarqueeOfFame, "Marquee of Fame", 2),
    new(HatTrick, "Hat Trick", 3),
    new(GrandSlam, "Grand Slam", 4),
    new(HighFive, "High Five", 5),
  ];

  public static TitleHonorificLevel? Find(string code) =>
    All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
}
