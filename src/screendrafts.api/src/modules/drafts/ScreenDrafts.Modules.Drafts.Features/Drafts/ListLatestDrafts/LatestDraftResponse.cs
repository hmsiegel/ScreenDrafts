namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListLatestDrafts;

internal sealed record LatestDraftResponse
{
  public string DraftPartPublicId { get; init; } = default!;
  public string DraftPublicId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public int? EpisodeNumber { get; init; }
  public int PartNumber { get; init; }
  public int TotalParts { get; init; }
  public DateOnly? ReleaseDate { get; init; }
  public IReadOnlyCollection<LatestDraftParticipantResponse> Participants { get; init; } = [];
}
