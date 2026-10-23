using ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts.PodcastLinks;

public class PodcastEpisodeMatcherTests
{
  private static readonly DateOnly _date = new(2026, 1, 15);
  private readonly PodcastEpisodeMatcher _matcher = new(new PodcastMatchSettings());

  [Fact]
  public void Match_ShouldReturnNoCandidates_WhenNothingIsNearTheDate()
  {
    var result = _matcher.Match(
      Context("Best Heist Movies"),
      [Episode("Best Heist Movies", _date.AddDays(10))]
    );

    result.Outcome.Should().Be(EpisodeMatchOutcome.NoCandidates);
  }

  [Fact]
  public void Match_ShouldReturnNoCandidates_WhenListIsEmpty()
  {
    var result = _matcher.Match(Context("Best Heist Movies"), []);

    result.Outcome.Should().Be(EpisodeMatchOutcome.NoCandidates);
  }

  [Fact]
  public void Match_ShouldMatch_WhenOneEpisodeIsWithinTolerance()
  {
    var episode = Episode("Whatever the title", _date.AddDays(2));

    var result = _matcher.Match(Context("Best Heist Movies"), [episode]);

    result.Outcome.Should().Be(EpisodeMatchOutcome.Matched);
    result.Episode.Should().Be(episode);
  }

  [Fact]
  public void Match_ShouldPreferExactDate_OverNearbyDates()
  {
    var exact = Episode("Other", _date);
    var near = Episode("Best Heist Movies", _date.AddDays(1));

    var result = _matcher.Match(Context("Best Heist Movies"), [near, exact]);

    result.Episode.Should().Be(exact);
  }

  [Fact]
  public void Match_ShouldPickByTitle_WhenSeveralShareTheDate()
  {
    var heist = Episode("Best Heist Movies with Sam and Jo", _date);
    var horror = Episode("Best Horror Sequels with Pat", _date);

    var result = _matcher.Match(Context("The Best Heist Movies"), [horror, heist]);

    result.Outcome.Should().Be(EpisodeMatchOutcome.Matched);
    result.Episode.Should().Be(heist);
  }

  [Fact]
  public void Match_ShouldBeAmbiguous_WhenTitlesScoreAlike()
  {
    var a = Episode("Best Movies Part One", _date);
    var b = Episode("Best Movies Part Two", _date);

    var result = _matcher.Match(Context("Best Movies"), [a, b]);

    result.Outcome.Should().Be(EpisodeMatchOutcome.Ambiguous);
  }

  [Fact]
  public void Match_ShouldBeAmbiguous_WhenNoTitleResemblesTheDraft()
  {
    var result = _matcher.Match(
      Context("Best Heist Movies"),
      [Episode("Alpha", _date), Episode("Beta", _date)]
    );

    result.Outcome.Should().Be(EpisodeMatchOutcome.Ambiguous);
  }

  private static ReleaseMatchContext Context(string title) => new(_date, title);

  private static EpisodeCandidate Episode(string title, DateOnly date) =>
    new(title, date, new Uri($"https://open.spotify.com/episode/{Guid.NewGuid():N}"));
}
