namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveDrafterFromDraft;

internal sealed class RemoveDrafterFromDraftCommandHandler(
  IDraftsRepository draftsRepository,
  IDraftersRepository draftersRepository,
  IDraftStatsRepository draftStatsRepository)
  : ICommandHandler<RemoveDrafterFromDraftCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IDraftStatsRepository _draftStatsRepository = draftStatsRepository;

  public async Task<Result<Guid>> Handle(RemoveDrafterFromDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);
    var drafterId = DrafterId.Create(request.DrafterId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var drafter = await _draftersRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId));
    }

    var result = draft.RemoveDrafter(drafter);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors);
    }

    var draftStats = await _draftStatsRepository.GetByDrafterAndDraftAsync(drafterId, draftId, cancellationToken);

    if (draftStats is null)
    {
      return Result.Failure<Guid>(DraftStatsErrors.NotFound(drafterId, draftId));
    }

    _draftsRepository.Update(draft);
    return Result.Success(drafterId.Value);
  }
}
