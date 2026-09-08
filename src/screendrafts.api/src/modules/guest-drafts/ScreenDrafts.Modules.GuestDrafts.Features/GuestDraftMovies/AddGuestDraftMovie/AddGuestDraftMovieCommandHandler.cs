using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDraftMovies.AddGuestDraftMovie;

internal sealed class AddGuestDraftMovieCommandHandler(IMovieRepository movieRepository)
  : ICommandHandler<AddGuestDraftMovieCommand, string>
{
  private readonly IMovieRepository _movieRepository = movieRepository;

  public async Task<Result<string>> Handle(
    AddGuestDraftMovieCommand request,
    CancellationToken cancellationToken
  )
  {
    var exists = await _movieRepository.ExistsByPublicIdAsync(request.PublicId, cancellationToken);

    if (exists)
    {
      // Idempotent skip -- MediaAddedIntegrationEvent can legitimately be
      // redelivered (outbox/inbox at-least-once), so this is an expected,
      // non-error outcome, not a real failure. Mirrors AddMovieCommandHandler's
      // MovieAlreadyExists handling, which the consumer logs and swallows.
      return Result.Failure<string>(MovieErrors.MovieAlreadyExists(request.PublicId));
    }

    var result = Movie.Create(
      movieTitle: request.Title,
      publicId: request.PublicId,
      id: request.Id,
      imdbId: request.ImdbId,
      tmdbId: request.TmdbId,
      igdbId: request.IgdbId,
      mediaType: request.MediaType,
      year: request.Year,
      tvSeriesTmdbId: request.TvSeriesTmdbId,
      seasonNumber: request.SeasonNumber,
      episodeNumber: request.EpisodeNumber,
      tvSeriesTitle: request.TvSeriesTitle
    );

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    var movie = result.Value;

    _movieRepository.Add(movie);

    return Result.Success(movie.PublicId);
  }
}
