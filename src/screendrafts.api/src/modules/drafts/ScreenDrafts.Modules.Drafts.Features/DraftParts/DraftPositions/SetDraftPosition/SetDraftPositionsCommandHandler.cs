namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.SetDraftPosition;

internal sealed class SetDraftPositionsCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<SetDraftPositionsCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(SetDraftPositionsCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    if (draftPart.GameBoard is null)
    {
      return Result.Failure(DraftPartErrors.GameBoardNotFound);
    }

    var gameBoard = draftPart.GameBoard;

    gameBoard.ClearPositions();

    List<DraftPosition> draftPositions = [];

    foreach (var position in request.Positions)
    {
      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPosition);

      var draftPosition = DraftPosition.Create(
        gameBoard,
        position.Name,
        position.Picks,
        publicId,
        position.HasBonusVeto,
        position.HasBonusVetoOverride);

      if (draftPosition.IsFailure)
      {
        return Result.Failure(draftPosition.Errors);
      }

      draftPositions.Add(draftPosition.Value);
    }

    var assignResult = gameBoard.AssignDraftPositions(draftPositions, draftPart.TotalDrafters + draftPart.TotalDrafterTeams);

    if (assignResult.IsFailure)
    {
      return assignResult;
    }

    _draftPartRepository.Update(draftPart);
    return Result.Success();
  }
}
