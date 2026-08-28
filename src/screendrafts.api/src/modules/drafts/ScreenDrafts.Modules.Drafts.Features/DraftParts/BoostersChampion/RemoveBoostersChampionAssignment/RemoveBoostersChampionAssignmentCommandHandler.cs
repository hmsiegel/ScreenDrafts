namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.RemoveBoostersChampionAssignment;

internal sealed class RemoveBoostersChampionAssignmentCommandHandler(
  IDraftPartRepository draftPartRepository
) : ICommandHandler<RemoveBoostersChampionAssignmentCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(
    RemoveBoostersChampionAssignmentCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var result = draftPart.RemoveBoostersChampionAssignment(request.AssignmentPublicId);

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
