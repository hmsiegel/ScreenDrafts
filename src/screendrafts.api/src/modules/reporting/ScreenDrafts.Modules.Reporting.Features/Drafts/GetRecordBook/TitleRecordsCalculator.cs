namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>Builds the Title section of the Record Book. Honorific-gap records are not included yet.</summary>
internal static class TitleRecordsCalculator
{
  public static RecordBookSection Build(IReadOnlyList<MediaRow> media)
  {
    ArgumentNullException.ThrowIfNull(media);

    var records = new List<RecordItem>();

    AddIfNotNull(
      records,
      RecordRanker.Build(
        "title.most-times-drafted",
        "Most times drafted",
        RecordRanker.Count,
        media,
        m => m.TimesDrafted,
        true,
        Holder));

    AddIfNotNull(
      records,
      RecordRanker.Build(
        "title.most-times-drafted-no1",
        "Most times drafted No. 1",
        RecordRanker.Count,
        media,
        m => m.TimesDraftedNo1,
        true,
        Holder));

    return new RecordBookSection
    {
      Key = "title",
      Title = "Title Records",
      Groups =
      [
        new RecordBookGroup { Key = "drafts", Title = "Drafts", Records = records },
      ],
    };
  }

  private static RecordHolder Holder(MediaRow m) =>
    new() { Kind = "title", Name = m.MediaTitle, PublicId = m.MediaPublicId };

  private static void AddIfNotNull(List<RecordItem> records, RecordItem? item)
  {
    if (item is not null)
    {
      records.Add(item);
    }
  }
}
