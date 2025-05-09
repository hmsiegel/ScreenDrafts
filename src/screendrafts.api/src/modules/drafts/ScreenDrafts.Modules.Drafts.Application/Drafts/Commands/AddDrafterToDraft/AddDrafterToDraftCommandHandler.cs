namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterToDraft;

internal sealed class AddDrafterToDraftCommandHandler(
  IDraftsRepository draftRepository,
  IDraftersRepository drafterRepository)
  : ICommandHandler<AddDrafterToDraftCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftRepository;
  private readonly IDraftersRepository _drafterRepository = drafterRepository;

  public async Task<Result<Guid>> Handle(AddDrafterToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var drafterId = DrafterId.Create(request.DrafterId);

    var drafter = await _drafterRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId));
    }

    draft.AddDrafter(drafter);
    _draftsRepository.Update(draft);
    return Result.Success(drafter.Id.Value);
  }
}
