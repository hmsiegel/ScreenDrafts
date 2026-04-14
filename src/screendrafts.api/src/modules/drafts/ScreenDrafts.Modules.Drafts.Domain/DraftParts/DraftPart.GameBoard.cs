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

    if (!HasParticipant(participant))
    {
      _draftPartParticipants.Add(DraftPartParticipant.Create(this, participant));
    }

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

  public Result AddSubDraft(int index, string publicId)
  {
    if (DraftType != DraftType.SpeedDraft)
    {
      return Result.Failure(SubDraftErrors.SubDraftsOnlyAllowedForSpeedDrafts);
    }

    if (_subDrafts.Any(s => s.Index == index))
    {
      return Result.Failure(DraftPartErrors.SubDraftIndexAlreadyExists(index));
    }

    var result = SubDraft.Create(
      index: index,
      draftPartId: Id,
      publicId: publicId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors[0]);
    }

    _subDrafts.Add(result.Value);
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public int StartingVetoesForSubDraft(int subDraftIndex, IReadOnlyCollection<(SubDraftId SubDraftId, bool IsOverridden)> vetoes)
  {
    if (DraftType != DraftType.SpeedDraft)
    {
      return 0;
    }

    var carry = 0;

    foreach (var subDraft in _subDrafts.OrderBy(s => s.Index).TakeWhile(s => s.Index < subDraftIndex))
    {
      carry = subDraft.ComputeVetoRemainder(1 + carry, vetoes);
    }

    return 1 + carry;
  }

  public Result<int> AdvanceSubDraft(SubDraftId subDraftId, IReadOnlyCollection<(SubDraftId SubDraftId, bool IsOverridden)> vetoes)
  {
    ArgumentNullException.ThrowIfNull(subDraftId);

    if (DraftType != DraftType.SpeedDraft)
    {
      return Result.Failure<int>(SubDraftErrors.SubDraftsOnlyAllowedForSpeedDrafts);
    }

    var current = _subDrafts.FirstOrDefault(s => s.Id == subDraftId);

    if (current is null)
    {
      return Result.Failure<int>(SubDraftErrors.NotFound(subDraftId.Value));
    }

    var completeResult = current.Complete();
    if (completeResult.IsFailure)
    {
      return Result.Failure<int>(completeResult.Errors[0]);
    }

    var next = _subDrafts
      .OrderBy(s => s.Index)
      .FirstOrDefault(s => s.Index > current.Index);

    if (next is null)
    {
      UpdatedAtUtc = DateTime.UtcNow;
      return Result.Success(0);
    }

    var startingVetoes = StartingVetoesForSubDraft(next.Index, vetoes);
    var remainder = next.ComputeVetoRemainder(startingVetoes, vetoes);

    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success(remainder);
  }
}
