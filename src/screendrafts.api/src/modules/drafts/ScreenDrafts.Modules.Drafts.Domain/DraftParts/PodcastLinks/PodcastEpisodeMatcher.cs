namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

/// <summary>
/// Picks the platform episode that belongs to a release.
/// <list type="number">
/// <item>Keep episodes within the tolerance window of the release date.</item>
/// <item>Prefer episodes on the exact release date.</item>
/// <item>One left: take it. Several: rank by how much of the draft title the
/// episode title contains, and take the leader only if it is clearly ahead.</item>
/// </list>
/// </summary>
public sealed class PodcastEpisodeMatcher(PodcastMatchSettings settings)
{
  private static readonly HashSet<string> _stopWords = new(StringComparer.Ordinal)
  {
    "a",
    "an",
    "and",
    "in",
    "of",
    "on",
    "the",
    "with",
  };

  private readonly PodcastMatchSettings _settings = settings;

  public EpisodeMatchResult Match(
    ReleaseMatchContext context,
    IReadOnlyList<EpisodeCandidate> episodes
  )
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(episodes);

    var nearby = episodes
      .Where(e =>
        Math.Abs(e.ReleaseDate.DayNumber - context.ReleaseDate.DayNumber)
        <= _settings.ToleranceDays
      )
      .ToList();

    if (nearby.Count == 0)
    {
      return EpisodeMatchResult.NoCandidates;
    }

    var exact = nearby.Where(e => e.ReleaseDate == context.ReleaseDate).ToList();
    var pool = exact.Count > 0 ? exact : nearby;

    return pool.Count == 1
      ? EpisodeMatchResult.Matched(pool[0])
      : PickByTitle(context.DraftTitle, pool);
  }

  private EpisodeMatchResult PickByTitle(string draftTitle, List<EpisodeCandidate> pool)
  {
    var draftTokens = Tokenize(draftTitle);

    var ranked = pool
      .Select(e => (Episode: e, Score: Score(draftTokens, e.Title)))
      .OrderByDescending(x => x.Score)
      .ToList();

    var best = ranked[0];
    var runnerUp = ranked[1];

    return best.Score >= _settings.MinTitleScore
      && best.Score - runnerUp.Score >= _settings.MinScoreMargin
      ? EpisodeMatchResult.Matched(best.Episode)
      : EpisodeMatchResult.Ambiguous;
  }

  /// <summary>
  /// Share of the draft title's words found in the episode title. Episode
  /// titles carry extra words (guest names), so containment beats overlap.
  /// </summary>
  private static double Score(HashSet<string> draftTokens, string episodeTitle)
  {
    if (draftTokens.Count == 0)
    {
      return 0;
    }

    var episodeTokens = Tokenize(episodeTitle);

    return draftTokens.Count(episodeTokens.Contains) / (double)draftTokens.Count;
  }

  private static HashSet<string> Tokenize(string text)
  {
    var cleaned = new string(
      text.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : ' ').ToArray()
    );

    return cleaned
      .Split(' ', StringSplitOptions.RemoveEmptyEntries)
      .Where(token => !_stopWords.Contains(token))
      .ToHashSet(StringComparer.Ordinal);
  }
}
