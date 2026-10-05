namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed record GetRecordBookResponse
{
  public required DateTime GeneratedAtUtc { get; init; }

  /// <summary>True when Patreon and Speed drafts are included alongside canonical drafts.</summary>
  public required bool IncludesNonCanonical { get; init; }

  public IReadOnlyList<RecordBookTotal> Totals { get; init; } = [];
  public IReadOnlyList<RecordBookSection> Sections { get; init; } = [];
}
