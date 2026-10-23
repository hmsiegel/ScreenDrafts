namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

/// <summary>
/// An episode found on a podcast platform, as the matcher sees it.
/// </summary>
public sealed record EpisodeCandidate(string Title, DateOnly ReleaseDate, Uri Url);
