namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;

public sealed record LatestDraftResponse(
  DraftId Id,
  Title Title,
  string EpisodeNumber,
  DateOnly ReleaseDate);
