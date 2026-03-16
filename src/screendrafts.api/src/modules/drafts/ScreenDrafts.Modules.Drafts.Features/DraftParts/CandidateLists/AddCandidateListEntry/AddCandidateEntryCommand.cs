namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.AddCandidateListEntry;

internal sealed record AddCandidateEntryCommand : ICommand<AddCanidateEntryResponse>
{
  public required string DraftPartId { get; init; }
  public required int TmdbId { get; init; }
  public string? Notes { get; init; }
  public required string AddedByPublicId { get; init; }
}
