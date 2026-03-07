namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

public sealed record DraftBrief
{
  public required string DraftPublicId { get; init; }
  public required string DraftTitle { get; init; }
  public IReadOnlyList<DateOnly> ReleaseDates { get; init; } = [];
}
