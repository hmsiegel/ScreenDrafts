namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>Builds the Draft section of the Record Book from per-part rows.</summary>
internal static class DraftRecordsCalculator
{
  private const string StandardDraftType = "Standard";

  public static RecordBookSection Build(
    IReadOnlyList<DraftPartRow> parts,
    IReadOnlyDictionary<Guid, int> uniqueTitlesPlayedByDraft,
    IReadOnlySet<Guid> copaceticDraftIds
  )
  {
    ArgumentNullException.ThrowIfNull(parts);
    ArgumentNullException.ThrowIfNull(uniqueTitlesPlayedByDraft);
    ArgumentNullException.ThrowIfNull(copaceticDraftIds);

    var partCounts = parts.GroupBy(p => p.DraftId).ToDictionary(g => g.Key, g => g.Count());
    var drafts = Summarize(parts, uniqueTitlesPlayedByDraft);

    return new RecordBookSection
    {
      Key = "draft",
      Title = "Draft Records",
      Groups =
      [
        new RecordBookGroup
        {
          Key = "titles",
          Title = "Titles",
          Records =
          [
            .. Compact(
              Draft("draft.most-titles-drafted", "Most titles drafted", drafts, d => d.PicksLanded),
              Part(
                "draft.most-titles-drafted-single-recording",
                "Most titles drafted, single recording",
                parts,
                partCounts,
                p => p.PicksLanded
              ),
              Draft(
                "draft.most-unique-titles-played",
                "Most unique titles played",
                drafts,
                d => d.UniqueTitlesPlayed
              ),
              Part(
                "draft.most-unique-titles-played-single-recording",
                "Most unique titles played, single recording",
                parts,
                partCounts,
                p => p.UniqueTitlesPlayed
              )
            ),
          ],
        },
        new RecordBookGroup
        {
          Key = "vetoes",
          Title = "Vetoes",
          Records =
          [
            .. Compact(
              Draft("draft.most-picks-vetoed", "Most picks vetoed", drafts, d => d.PicksVetoed),
              Part(
                "draft.most-picks-vetoed-single-recording",
                "Most picks vetoed, single recording",
                parts,
                partCounts,
                p => p.PicksVetoed
              ),
              Draft(
                "draft.most-picks-vetoed-non-expanded",
                "Most picks vetoed, non-expanded draft",
                drafts.Where(d => d.DraftType == StandardDraftType).ToList(),
                d => d.PicksVetoed
              ),
              Draft(
                "draft.most-no1-picks-vetoed",
                "Most No. 1 picks vetoed",
                drafts,
                d => d.No1Vetoed
              )
            ),
          ],
        },
        new RecordBookGroup
        {
          Key = "veto-overrides",
          Title = "Veto Overrides",
          Records =
          [
            .. Compact(
              Draft(
                "draft.most-vetoes-overridden",
                "Most vetoes overridden",
                drafts,
                d => d.VetoesOverridden
              ),
              Part(
                "draft.most-vetoes-overridden-single-recording",
                "Most vetoes overridden, single recording",
                parts,
                partCounts,
                p => p.VetoesOverridden
              )
            ),
          ],
        },
        new RecordBookGroup
        {
          Key = "commissioner-overrides",
          Title = "Commissioner Overrides",
          Records =
          [
            .. Compact(
              Draft(
                "draft.most-commissioner-overrides",
                "Most commissioner overrides",
                drafts,
                d => d.CommissionerOverrides
              )
            ),
          ],
        },
        new RecordBookGroup
        {
          Key = "copacetic",
          Title = "Copacetic Drafts",
          Records =
          [
            .. Compact(
              Draft(
                "draft.most-titles-drafted-in-a-copacetic-draft",
                "Most titles drafted in a copacetic draft",
                drafts.Where(d => copaceticDraftIds.Contains(d.DraftId)).ToList(),
                d => d.PicksLanded
              )
            ),
          ],
        },
      ],
    };
  }

  private static RecordItem? Draft(
    string code,
    string label,
    IReadOnlyList<DraftSummaryRow> drafts,
    Func<DraftSummaryRow, decimal> metric
  ) =>
    RecordRanker.Build(
      code,
      label,
      RecordRanker.Count,
      drafts,
      metric,
      true,
      d => new RecordHolder
      {
        Kind = "draft",
        Name = d.DraftTitle,
        PublicId = d.DraftPublicId,
      }
    );

  private static RecordItem? Part(
    string code,
    string label,
    IReadOnlyList<DraftPartRow> parts,
    Dictionary<Guid, int> partCounts,
    Func<DraftPartRow, decimal> metric
  ) =>
    RecordRanker.Build(
      code,
      label,
      RecordRanker.Count,
      parts,
      metric,
      true,
      p => new RecordHolder
      {
        Kind = "draft",
        Name = p.DraftTitle,
        PublicId = p.DraftPublicId,
        Context = partCounts[p.DraftId] > 1 ? $"Part {p.PartIndex}" : null,
      }
    );

  private static List<DraftSummaryRow> Summarize(
    IReadOnlyList<DraftPartRow> parts,
    IReadOnlyDictionary<Guid, int> uniqueTitlesPlayedByDraft
  ) =>
    [
      .. parts
        .GroupBy(p => p.DraftId)
        .Select(g => new DraftSummaryRow
        {
          DraftId = g.Key,
          DraftPublicId = g.First().DraftPublicId,
          DraftTitle = g.First().DraftTitle,
          DraftType = g.First().DraftType,
          PicksLanded = g.Sum(p => p.PicksLanded),
          UniqueTitlesPlayed = uniqueTitlesPlayedByDraft.GetValueOrDefault(g.Key),
          PicksVetoed = g.Sum(p => p.PicksVetoed),
          No1Vetoed = g.Sum(p => p.No1Vetoed),
          CommissionerOverrides = g.Sum(p => p.CommissionerOverrides),
          VetoesOverridden = g.Sum(p => p.VetoesOverridden),
        }),
    ];

  private static IEnumerable<RecordItem> Compact(params RecordItem?[] items) =>
    items.OfType<RecordItem>();
}
