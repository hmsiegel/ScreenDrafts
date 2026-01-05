namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed partial class DraftPart
{
  internal Result<PickId> AddPick(Pick pick)
  {
    Guard.Against.Null(pick);

    if (Status != DraftStatus.InProgress)
    {
      return Result.Failure<PickId>(DraftErrors.DraftNotStarted);
    }

    if (_picks.Any(p => p.Position == pick.Position))
    {
      return Result.Failure<PickId>(DraftErrors.PickPositionAlreadyTaken(pick.Position));
    }

    if (pick.Position <= 0 || pick.Position > TotalPicks)
    {
      return Result.Failure<PickId>(DraftErrors.PickPositionIsOutOfRange);
    }

    if (pick.PlayOrder <= 0)
    {
      return Result.Failure<PickId>(DraftErrors.PickPlayOrderIsOutOfRange);
    }

    _picks.Add(pick);
    _picks.Sort((a, b) => a.Position.CompareTo(b.Position));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new PickAddedDomainEvent(
      Id.Value,
      pick.Position,
      pick.Movie.Id,
      pick.Drafter!.Id.Value,
      pick.DrafterTeam!.Id.Value,
      pick.PlayOrder,
      Id.Value));

    return Result.Success(pick.Id);
  }

  internal Result ApplyCommissionerOverride(Pick pick)
  {
    ArgumentNullException.ThrowIfNull(pick);

    var overrideEntry = CommissionerOverride.Create(pick).Value;

    pick.ApplyCommissionerOverride(overrideEntry);

    var drafterStats = _drafterDraftStats
      .FirstOrDefault(d => d.Drafter?.Id.Value == pick.DrafterId?.Value);

    drafterStats?.AddCommissionerOverride();

    return Result.Success();
  }

  internal Result RecordVeto(PickId pickId, VetoIssuerKind issuerKind, ParticipantId? participantId)
  {
    var pick = _picks.FirstOrDefault(p => p.Id == pickId);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFound(pickId.Value));
    }

    var drafterStats = _drafterDraftStats
      .FirstOrDefault(d => d.Drafter?.Id.Value == pick.DrafterId?.Value);

    drafterStats?.SetUsedBlessing(1, isVeto: true);

    var veto = Veto.Create(pick, issuerKind, participantId).Value;

    return pick.ApplyVeto(veto);
  }

  internal Result RecordVetoOverride(Veto veto, ParticipantId by)
  {
    var pick = _picks.FirstOrDefault(p => p.Id.Value == veto.PickId.Value);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFound(veto.PickId.Value));
    }

    var drafterStats = _drafterDraftStats
      .FirstOrDefault(d => d.Drafter?.Id.Value == pick.DrafterId?.Value);

    drafterStats?.SetUsedBlessing(1, isVeto: false);

    return pick.ApplyVetoOverride(veto, by);
  }

  internal Result SetDrafterTeams(IReadOnlyList<DrafterTeam> drafterTeamIds)
  {
    _drafterTeams.Clear();
    _drafterTeams.AddRange(drafterTeamIds);
    return Result.Success();
  }

  internal Result SetHosts(IReadOnlyList<DraftHost> hosts)
  {
    _draftHosts.Clear();
    _draftHosts.AddRange(hosts);
    return Result.Success();
  }

  internal void SetPartIndex(int partIndex)
  {
    PartIndex = partIndex;
  }
}
