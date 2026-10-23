namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

/// <summary>
/// What the matcher knows about the release it is looking for.
/// </summary>
public sealed record ReleaseMatchContext(DateOnly ReleaseDate, string DraftTitle);
