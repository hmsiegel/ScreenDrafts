namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDraftMovies.AddGuestDraftMovie;

internal sealed record AddGuestDraftMovieCommand : ICommand<string>
{
  public Guid Id { get; init; }
  public string PublicId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public string? ImdbId { get; init; }
  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public MediaType MediaType { get; init; } = default!;
  public string? Year { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
  public string? TvSeriesTitle { get; init; }
}
