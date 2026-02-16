using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddDrafterToDraft;

internal sealed class AddDrafterToDraftCommandHandler(
  IDraftRepository draftRepository,
  IDraftersRepository drafterRepository)
  : ICommandHandler<AddDrafterToDraftCommand, Guid>
{
  private readonly IDraftRepository _draftsRepository = draftRepository;
  private readonly IDraftersRepository _drafterRepository = drafterRepository;

  public async Task<Result<Guid>> Handle(AddDrafterToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftPartId = DraftPartId.Create(request.DraftPartId);

    var draft = await _draftsRepository.GetDraftByDraftPartId(draftPartId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftPartId));
    }

    var draftPart = await _draftsRepository.GetDraftPartByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<Guid>(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    var drafterId = DrafterId.Create(request.DrafterId);

    var drafter = await _drafterRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId));
    }

    draftPart.AddDrafter(drafter);
    _draftsRepository.Update(draft);
    return Result.Success(drafter.Id.Value);
  }
}
