namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.AssignFilmToBoostersChampionAssignment;

internal sealed class AssignFilmToBoostersChampionAssignmentCommandHandler(
  IDraftPartRepository draftPartRepository
) : ICommandHandler<AssignFilmToBoostersChampionAssignmentCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(
    AssignFilmToBoostersChampionAssignmentCommand request,
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

    var result = draftPart.AssignFilmToBoostersChampionAssignment(
      request.AssignmentPublicId,
      request.TmdbId
    );

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
