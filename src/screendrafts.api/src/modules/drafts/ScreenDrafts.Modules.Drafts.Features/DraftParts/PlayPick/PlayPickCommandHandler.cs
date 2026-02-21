using ScreenDrafts.Modules.Drafts.Features.Common;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.PlayPick;

internal sealed class PlayPickCommandHandler(
  IDraftPartRepository draftPartRepository,
  IMovieRepository movieRepository,
  ISeriesPolicyProvider seriesPolicyProvider,
  ParticipantResolver participantResolver)
  : ICommandHandler<PlayPickCommand, PickId>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly ISeriesPolicyProvider _seriesPolicyProvider = seriesPolicyProvider;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<Result<PickId>> Handle(PlayPickCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByIdAsync(DraftPartId.Create(request.DraftPartId), cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<PickId>(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

    if (movie is null)
    {
      return Result.Failure<PickId>(MovieErrors.NotFound(request.MovieId));
    }

    var participantResult = await _participantResolver.ResolveAsync(
      request.ParticipantPublicId,
      request.ParticipantKind,
      cancellationToken);

    if (participantResult.IsFailure)
    {
      return Result.Failure<PickId>(participantResult.Errors);
    }

    var participant = participantResult.Value;

    participant.Validate();

    var pickResult = draftPart.PlayPick(
      seriesPolicyProvider: _seriesPolicyProvider,
      seriesId: draftPart.SeriesId,
      draftType: draftPart.DraftType,
      movie: movie,
      draftPosition: request.Position,
      playOrder: request.PlayOrder,
      participantId: participant,
      movieVersionName: request.MovieVersionName);

    if (pickResult.IsFailure)
    {
       return Result.Failure<PickId>(pickResult.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success(pickResult.Value);
  }

}
