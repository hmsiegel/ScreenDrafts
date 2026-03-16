namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.RemoveCandidateListEntry;

internal sealed record RemoveCandidateListEntryCommand : ICommand
{
  public required string DraftPartId { get; init; }

  public required int TmdbId { get; init; }
}
