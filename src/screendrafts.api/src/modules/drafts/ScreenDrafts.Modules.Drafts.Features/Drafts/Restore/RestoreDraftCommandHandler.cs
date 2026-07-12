namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Restore;

internal sealed class RestoreDraftCommandHandler(
  IDraftRepository draftsRepository,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<RestoreDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(RestoreDraftCommand request, CancellationToken cancellationToken)
  {
    // Deleted drafts are excluded by Draft's global query filter — restoring
    // one requires reading past that filter, or GetDraftByPublicId will
    // never find it in the first place. See note on IDraftRepository /
    // DraftConfiguration: needs an IgnoreQueryFilters()-based lookup here.
    var draft = await _draftsRepository.GetDraftByPublicIdIncludingDeletedAsync(
      request.PublicId,
      cancellationToken
    );

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.PublicId));
    }

    var result = draft.Restore(_dateTimeProvider.UtcNow);

    if (result.IsFailure)
    {
      return result;
    }

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}
