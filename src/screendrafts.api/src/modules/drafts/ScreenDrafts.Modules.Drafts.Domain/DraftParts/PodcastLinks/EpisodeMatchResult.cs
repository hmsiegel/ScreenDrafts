namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

public sealed record EpisodeMatchResult(EpisodeMatchOutcome Outcome, EpisodeCandidate? Episode = null)
{
  public static readonly EpisodeMatchResult NoCandidates = new(EpisodeMatchOutcome.NoCandidates);

  public static readonly EpisodeMatchResult Ambiguous = new(EpisodeMatchOutcome.Ambiguous);

  public static EpisodeMatchResult Matched(EpisodeCandidate episode) =>
    new(EpisodeMatchOutcome.Matched, episode);
}
