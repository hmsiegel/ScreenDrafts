namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Update;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand
{
  public required string PublicId { get; init; }
  public string? Title { get; init; } = default!;
  public string? Description { get; init; } = default!;
  public string? SeriesPublicId { get; init; } = default!;
  public string? CampaignPublicId { get; init; } = default!;
  public IReadOnlyList<string>? PublicCategoryIds { get; init; } = [];
  public int DraftTypeValue { get; init; } = default!;
}
