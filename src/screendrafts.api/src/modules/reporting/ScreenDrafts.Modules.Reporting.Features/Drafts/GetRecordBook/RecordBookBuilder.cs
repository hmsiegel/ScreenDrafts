namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal static class RecordBookBuilder
{
  public static GetRecordBookResponse Build(
    RecordBookData data,
    bool includesNonCanonical,
    DateTime generatedAtUtc
  )
  {
    ArgumentNullException.ThrowIfNull(data);

    var copacetic = CopaceticDrafts.Find(data.Parts);

    return new GetRecordBookResponse
    {
      GeneratedAtUtc = generatedAtUtc,
      IncludesNonCanonical = includesNonCanonical,
      Totals = RecordBookTotalsCalculator.Build(data, copacetic),
      Sections =
      [
        DrafterRecordsCalculator.Build(data.DrafterDrafts, data.PickSlots, copacetic),
        DraftRecordsCalculator.Build(data.Parts, data.UniqueTitlesPlayedByDraft, copacetic),
        TitleRecordsCalculator.Build(data.Media),
      ],
    };
  }
}
