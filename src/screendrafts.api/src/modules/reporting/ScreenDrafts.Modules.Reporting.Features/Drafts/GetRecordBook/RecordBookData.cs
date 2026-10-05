namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>Everything the calculators need, loaded once per request.</summary>
internal sealed record RecordBookData
{
  public required IReadOnlyList<DrafterDraftRow> DrafterDrafts { get; init; }
  public required IReadOnlyList<DraftPartRow> Parts { get; init; }
  public required IReadOnlyList<MediaRow> Media { get; init; }
  public required IReadOnlyList<PickSlotRow> PickSlots { get; init; }
  public required int VetoesStood { get; init; }
  public required int VetoesOverridden { get; init; }
  public required int SelfVetoesStood { get; init; }
  public required int MarqueeOfFameTitles { get; init; }
  public required int HatTrickTitles { get; init; }
  public required int GrandSlamTitles { get; init; }
}
