namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;

public sealed partial class DraftPart
{
  public Result AssignParticipantToPosition(DraftPosition position, Participant participant)
  {
    ArgumentNullException.ThrowIfNull(position);

    if (GameBoard is null)
    {
      return Result.Failure(DraftPartErrors.GameBoardNotFound);
    }

    if (position.GameBoardId != GameBoard.Id)
    {
      return Result.Failure(DraftPositionErrors.PositionDoesNotBelongToThisGameBoard);
    }

    if (HasParticipant(participant))
    {
      return Result.Failure(DraftPartErrors.ParticipantAlreadyAdded(participant.Value));
    }

    _draftPartParticipants.Add(DraftPartParticipant.Create(this, participant));

    var assignResult = position.AssignParticipant(participant);

    if (assignResult.IsFailure)
    {
      return assignResult;
    }

    if (position.HasBonusVeto)
    {
      var awardResult = SetParticipantAward(participant, isVeto: true);
      if (awardResult.IsFailure)
      {
        return awardResult;
      }
    }

    if (position.HasBonusVetoOverride)
    {
      var awardResult = SetParticipantAward(participant, isVeto: false);
      if (awardResult.IsFailure)
      {
        return awardResult;
      }
    }

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result ClearPositionAssignment(DraftPosition position)
  {
    ArgumentNullException.ThrowIfNull(position);

    if (GameBoard is null)
    {
      return Result.Failure(DraftPartErrors.GameBoardNotFound);
    }

    if (position.GameBoardId != GameBoard.Id)
    {
      return Result.Failure(DraftPositionErrors.PositionDoesNotBelongToThisGameBoard);
    }

    if (position.AssignedTo is null)
    {
      return Result.Failure(DraftPositionErrors.PositionIsNotAssigned);
    }

    var participant = position.AssignedTo.Value;

    var clearResult = position.ClearAssignment();

    if (clearResult.IsFailure)
    {
      return clearResult;
    }

    if (HasParticipant(participant))
    {
      if (position.HasBonusVeto)
      {
        var revokeResult = RevokeParticipantAward(participant, isVeto: true);

        if (revokeResult.IsFailure) { return revokeResult; }
      }

      if (position.HasBonusVetoOverride)
      {
        var revokeResult = RevokeParticipantAward(participant, isVeto: false);

        if (revokeResult.IsFailure) { return revokeResult; }
      }
    }

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }
}
