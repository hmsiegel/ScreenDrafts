namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal static class RecordBookTotalsCalculator
{
  public static IReadOnlyList<RecordBookTotal> Build(
    RecordBookData data,
    IReadOnlySet<Guid> copaceticDraftIds)
  {
    ArgumentNullException.ThrowIfNull(data);
    ArgumentNullException.ThrowIfNull(copaceticDraftIds);

    return
    [
      Total("drafts", "Drafts", data.Parts.Select(p => p.DraftId).Distinct().Count()),
      Total("picks-made", "Picks made", data.Parts.Sum(p => p.PicksLanded)),
      Total("unique-titles-drafted", "Unique titles drafted", data.Media.Count),
      Total("vetoes-deployed", "Vetoes successfully deployed", data.VetoesStood),
      Total("vetoes-overridden", "Vetoes overridden", data.VetoesOverridden),
      Total("commissioner-overrides", "Commissioner overrides", data.Parts.Sum(p => p.CommissionerOverrides)),
      Total("copacetic-drafts", "Copacetic drafts", copaceticDraftIds.Count),
      Total("self-vetoes", "Self-vetoes", data.SelfVetoesStood),
      Total(
        "unique-guest-gms",
        "Unique Guest G.M.s",
        data.DrafterDrafts.Where(r => r.Appeared).Select(r => r.DrafterId).Distinct().Count()),
      Total("marquee-of-fame-titles", "Marquee of Fame titles", data.MarqueeOfFameTitles),
      Total("hat-trick-titles", "Hat Trick titles", data.HatTrickTitles),
      Total("grand-slam-titles", "Grand Slam titles", data.GrandSlamTitles),
    ];
  }

  private static RecordBookTotal Total(string code, string label, int value) =>
    new() { Code = code, Label = label, Value = value };
}
