namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Update;

internal sealed record UpdateDraftCommand : ICommand
{
  public required string PublicId { get; init; }
  public string? Title { get; init; } = default!;
  public string? Description { get; init; } = default!;
  public string? SeriesPublicId { get; init; } = default!;
  public string? CampaignPublicId { get; init; } = default!;
  public IReadOnlyList<string>? PublicCategoryIds { get; init; } = [];
  public int DraftTypeValue { get; init; } = default!;

  /// <summary>
  /// Flavor name for this draft's fungible veto/override token. Null/omitted leaves it
  /// unset (or clears it, if a part hasn't started yet — see UpdateDraftCommandHandler's
  /// guard once a part has started).
  /// </summary>
  public string? FungibleTokenName { get; init; }
  public bool? IsHostless { get; init; }
}
