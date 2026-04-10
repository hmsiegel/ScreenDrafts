namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.PlaySubDraftPick;

internal sealed class PlaySubDraftPickCommandHandler(
  IDraftPartRepository draftPartRepository,
  IMovieRepository movieRepository,
  ParticipantResolver participantResolver,
  ISeriesPolicyProvider seriesPolicyProvider)
  : ICommandHandler<PlaySubDraftPickCommand, PickId>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly ISeriesPolicyProvider _seriesPolicyProvider = seriesPolicyProvider;

  public async Task<Result<PickId>> Handle(PlaySubDraftPickCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<PickId>(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var subDraft = draftPart.SubDrafts
     .FirstOrDefault(s => s.PublicId == request.SubDraftPublicId);

    if (subDraft is null)
    {
      return Result.Failure<PickId>(SubDraftErrors.NotFound(request.SubDraftPublicId));
    }

    if (subDraft.Status != SubDraftStatus.Active)
    {
      return Result.Failure<PickId>(SubDraftErrors.MustBeActive);
    }

    var series = await _seriesPolicyProvider.GetSeriesAsyc(draftPart.SeriesId, cancellationToken);

    if (series is null)
    {
      return Result.Failure<PickId>(SeriesErrors.SeriesNotFound(draftPart.SeriesId.Value));
    }

    var movie = await _movieRepository.GetByPublicIdAsync(request.MoviePublicId, cancellationToken);

    if (movie is null)
    {
      return Result.Failure<PickId>(MovieErrors.NotFound(request.MoviePublicId));
    }

    var participantResult = await _participantResolver.ResolveAsync(
      participantPublicId: request.ParticipantPublicId,
      participantKind: request.ParticipantKind,
      ct: cancellationToken);

    if (participantResult.IsFailure)
    {
      return Result.Failure<PickId>(participantResult.Errors);
    }

    var participant = participantResult.Value;

    var validationResult = participant.Validate();

    if (validationResult.IsFailure)
    {
      return Result.Failure<PickId>(validationResult.Errors);
    }

    var pickResult = draftPart.PlayPick(
      movie: movie,
      draftPosition: request.Position,
      playOrder: request.PlayOrder,
      participantId: participant,
      canonicalPolicyValue: CanonicalPolicy.FromValue(series.CanonicalPolicy.Value),
      subDraftId: subDraft.Id,
      actedByPublicId: request.ActedByPublicId);

    if (pickResult.IsFailure)
    {
      return Result.Failure<PickId>(pickResult.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success(pickResult.Value);
  }
}
