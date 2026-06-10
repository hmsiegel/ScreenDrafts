namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.RemoveMovie;

// ── Handler ───────────────────────────────────────────────────────────────────

internal sealed class RemoveMovieFromDraftPoolCommandHandler(
  IDraftRepository draftRepository,
  IDraftPoolRepository poolRepository
) : ICommandHandler<RemoveMovieFromDraftPoolCommand>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IDraftPoolRepository _poolRepository = poolRepository;

  public async Task<Result> Handle(
    RemoveMovieFromDraftPoolCommand request,
    CancellationToken cancellationToken
  )
  {
    var draft = await _draftRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.PublicId));
    }

    var pool = await _poolRepository.GetByDraftIdAsync(draft.Id, cancellationToken);

    if (pool is null)
    {
      return Result.Failure(DraftPoolErrors.NotFound(request.PublicId));
    }

    var removeResult = pool.RemoveMovie(request.TmdbId);

    if (removeResult.IsFailure)
    {
      return removeResult;
    }

    _poolRepository.Update(pool);

    return Result.Success();
  }
}
