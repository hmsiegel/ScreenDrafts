namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetTvSeriesRestriction;

internal sealed class SetTvSeriesRestrictionCommandHandler(IDraftRepository draftRepository)
  : ICommandHandler<SetTvSeriesRestrictionCommand>
{
  private readonly IDraftRepository _draftRepository = draftRepository;

  public async Task<Result> Handle(
    SetTvSeriesRestrictionCommand request,
    CancellationToken cancellationToken
  )
  {
    var draft = await _draftRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.PublicId));
    }

    draft.SetTvSeriesRestriction(request.TvSeriesTmdbId);

    _draftRepository.Update(draft);

    return Result.Success();
  }
}
