using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.RemoveDrafterFromDraftPart;

internal sealed class RemoveDrafterFromDraftPartCommandHandler(
  IDraftRepository draftsRepository,
  IDraftersRepository draftersRepository,
  IDraftStatsRepository draftStatsRepository)
  : ICommandHandler<RemoveDrafterFromDraftPartCommand, Guid>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IDraftStatsRepository _draftStatsRepository = draftStatsRepository;

  public async Task<Result<Guid>> Handle(RemoveDrafterFromDraftPartCommand request, CancellationToken cancellationToken)
  {
    var draftPartId = DraftPartId.Create(request.DraftPartId);
    var drafterId = DrafterId.Create(request.DrafterId);

    var draftPart = await _draftsRepository.GetDraftPartByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<Guid>(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    var drafter = await _draftersRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId));
    }

    var result = draftPart.RemoveDrafter(drafter);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors);
    }

    var draftStats = await _draftStatsRepository.GetByDrafterAndDraftPartAsync(drafterId, draftPartId, cancellationToken);

    if (draftStats is null)
    {
      return Result.Failure<Guid>(DraftStatsErrors.NotFound(request.DrafterId, request.DraftPartId));
    }

    var draft = await _draftsRepository.GetDraftByDraftPartId(draftPart.Id, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(draft!.Id.Value));
    }

    _draftsRepository.Update(draft);
    return Result.Success(drafterId.Value);
  }
}
