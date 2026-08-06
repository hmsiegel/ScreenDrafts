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

    var allAssigned = GameBoard.DraftPositions.All(p => p.AssignedTo is not null);

    if (allAssigned)
    {
      Raise(new AllPositionsAssignedDomainEvent(Id.Value, PublicId));
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

        if (revokeResult.IsFailure)
        {
          return revokeResult;
        }
      }

      if (position.HasBonusVetoOverride)
      {
        var revokeResult = RevokeParticipantAward(participant, isVeto: false);

        if (revokeResult.IsFailure)
        {
          return revokeResult;
        }
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

    if (_draftPartParticipants.Count != 2)
    {
      return Result.Failure(SubDraftErrors.SpeedDraftRequiresExactlyTwoParticipants);
    }

    if (_subDrafts.Any(s => s.Index == index))
    {
      return Result.Failure(DraftPartErrors.SubDraftIndexAlreadyExists(index));
    }

    var result = SubDraft.Create(index: index, draftPartId: Id, publicId: publicId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors[0]);
    }

    _subDrafts.Add(result.Value);
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result SetSpeedDraftPositions(
    IReadOnlyList<(
      string SubDraftPublicId,
      string PositionPublicId,
      string Name,
      int[] Picks
    )> seeds
  )
  {
    if (DraftType != DraftType.SpeedDraft)
    {
      return Result.Failure(SubDraftErrors.SubDraftsOnlyAllowedForSpeedDrafts);
    }

    foreach (var group in seeds.GroupBy(s => s.SubDraftPublicId))
    {
      var subDraft = _subDrafts.FirstOrDefault(s => s.PublicId == group.Key);

      if (subDraft is null)
      {
        return Result.Failure(SubDraftErrors.NotFound(group.Key));
      }

      var board = subDraft.GameBoard;

      if (board is null)
      {
        return Result.Failure(DraftPartErrors.GameBoardNotFound);
      }

      if (board.DraftPositions.Count > 0)
      {
        return Result.Failure(SubDraftErrors.PositionsAlreadySet);
      }

      var created = new List<DraftPosition>();

      foreach (var seed in group)
      {
        var positionResult = DraftPosition.Create(
          gameBoard: board,
          name: seed.Name,
          picks: seed.Picks,
          publicId: seed.PositionPublicId
        );

        if (positionResult.IsFailure)
        {
          return Result.Failure(positionResult.Errors);
        }

        created.Add(positionResult.Value);
      }

      var assignResult = board.AssignDraftPositions(created);

      if (assignResult.IsFailure)
      {
        return assignResult;
      }
    }

    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result AssignSubDraftPosition(
    SubDraftId subDraftId,
    Participant winner,
    string winnerChoice
  )
  {
    ArgumentNullException.ThrowIfNull(subDraftId);

    if (DraftType != DraftType.SpeedDraft)
    {
      return Result.Failure(SubDraftErrors.SubDraftsOnlyAllowedForSpeedDrafts);
    }

    var subDraft = _subDrafts.FirstOrDefault(s => s.Id == subDraftId);

    if (subDraft is null)
    {
      return Result.Failure(SubDraftErrors.NotFound(subDraftId.Value));
    }

    if (subDraft.Status != SubDraftStatus.Active)
    {
      return Result.Failure(SubDraftErrors.MustBeActive);
    }

    if (_draftPartParticipants.Count != 2)
    {
      return Result.Failure(SubDraftErrors.SpeedDraftRequiresExactlyTwoParticipants);
    }

    if (!HasParticipant(winner))
    {
      return Result.Failure(DraftPartErrors.ParticipantDoesNotBelongToThisDraftPart(winner));
    }

    if (winnerChoice != "A" && winnerChoice != "B")
    {
      return Result.Failure(SubDraftErrors.InvalidPositionChoice);
    }

    var board = subDraft.GameBoard;

    if (board is null)
    {
      return Result.Failure(DraftPartErrors.GameBoardNotFound);
    }

    var loserChoice = winnerChoice == "A" ? "B" : "A";
    var loser = _draftPartParticipants.Select(dp => dp.ParticipantId).First(p => p != winner);

    var winnerPosition = board.DraftPositions.FirstOrDefault(p => p.Name == winnerChoice);
    var loserPosition = board.DraftPositions.FirstOrDefault(p => p.Name == loserChoice);

    if (winnerPosition is null || loserPosition is null)
    {
      // Positions weren't set up — SetSpeedDraftPositions was never called
      // for this sub-draft, or hasn't run yet.
      return Result.Failure(GameBoardErrors.DraftPositionsMissing);
    }

    if (winnerPosition.AssignedTo is not null || loserPosition.AssignedTo is not null)
    {
      return Result.Failure(SubDraftErrors.PositionsAlreadyChosen);
    }

    var winnerAssignResult = winnerPosition.AssignParticipant(winner);

    if (winnerAssignResult.IsFailure)
    {
      return winnerAssignResult;
    }

    var loserAssignResult = loserPosition.AssignParticipant(loser);

    if (loserAssignResult.IsFailure)
    {
      return loserAssignResult;
    }

    Raise(
      new SubDraftUpdatedDomainEvent(
        draftPartId: Id.Value,
        draftPartPublicId: PublicId,
        subDraftId: subDraft.Id.Value,
        subDraftPublicId: subDraft.PublicId,
        status: subDraft.Status.Value,
        subjectKind: subDraft.SubjectKind?.Value,
        subjectName: subDraft.SubjectName,
        subjectImdbId: subDraft.SubjectImdbId
      )
    );

    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public int StartingVetoesForSubDraft(
    Participant participant,
    int subDraftIndex,
    IReadOnlyCollection<(SubDraftId SubDraftId, Participant IssuedBy)> vetoes
  )
  {
    if (DraftType != DraftType.SpeedDraft)
    {
      return 0;
    }

    var carry = 0;

    foreach (
      var subDraft in _subDrafts.OrderBy(s => s.Index).TakeWhile(s => s.Index < subDraftIndex)
    )
    {
      carry = subDraft.ComputeVetoRemainder(participant, 1 + carry, vetoes);
    }

    return 1 + carry;
  }

  public Result<IReadOnlyDictionary<Participant, int>> AdvanceSubDraft(
    SubDraftId subDraftId,
    IReadOnlyCollection<(SubDraftId SubDraftId, Participant IssuedBy)> vetoes
  )
  {
    ArgumentNullException.ThrowIfNull(subDraftId);

    if (DraftType != DraftType.SpeedDraft)
    {
      return Result.Failure<IReadOnlyDictionary<Participant, int>>(
        SubDraftErrors.SubDraftsOnlyAllowedForSpeedDrafts
      );
    }

    var current = _subDrafts.FirstOrDefault(s => s.Id == subDraftId);

    if (current is null)
    {
      return Result.Failure<IReadOnlyDictionary<Participant, int>>(
        SubDraftErrors.NotFound(subDraftId.Value)
      );
    }

    var completeResult = current.Complete();
    if (completeResult.IsFailure)
    {
      return Result.Failure<IReadOnlyDictionary<Participant, int>>(completeResult.Errors[0]);
    }

    Raise(
      new SubDraftUpdatedDomainEvent(
        draftPartId: Id.Value,
        draftPartPublicId: PublicId,
        subDraftId: current.Id.Value,
        subDraftPublicId: current.PublicId,
        status: current.Status.Value,
        subjectKind: current.SubjectKind?.Value,
        subjectName: current.SubjectName,
        subjectImdbId: current.SubjectImdbId
      )
    );

    var next = _subDrafts.OrderBy(s => s.Index).FirstOrDefault(s => s.Index > current.Index);

    if (next is null)
    {
      UpdatedAtUtc = DateTime.UtcNow;

      Raise(
        new AllSubDraftsCompletedDomainEvent(
          draftId: DraftId.Value,
          draftPartId: Id.Value,
          draftPublicId: DraftPublicId,
          draftPartPublicId: PublicId
        )
      );

      return Result.Success<IReadOnlyDictionary<Participant, int>>(
        new Dictionary<Participant, int>()
      );
    }

    var remainders = new Dictionary<Participant, int>();

    foreach (var participant in _draftPartParticipants.Select(dp => dp.ParticipantId))
    {
      var startingVetoesForNext = StartingVetoesForSubDraft(participant, next.Index, vetoes);
      remainders[participant] = startingVetoesForNext - 1;
    }

    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success<IReadOnlyDictionary<Participant, int>>(remainders);
  }
}
