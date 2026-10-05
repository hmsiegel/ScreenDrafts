namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed record RecordItem
{
  public required string Code { get; init; }
  public required string Label { get; init; }

  /// <summary>"count", "ratio" (per draft) or "percent" (0 to 100).</summary>
  public required string Format { get; init; }
  public required decimal Value { get; init; }

  /// <summary>For example "10+ drafts" on records that need a minimum number of appearances.</summary>
  public string? Qualifier { get; init; }

  /// <summary>Everyone sharing the record. Ties return every holder.</summary>
  public IReadOnlyList<RecordHolder> Holders { get; init; } = [];
}
