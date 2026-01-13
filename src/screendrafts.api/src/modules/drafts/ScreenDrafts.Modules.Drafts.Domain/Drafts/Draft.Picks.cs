namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed partial class Draft
{
  /// <summary>
  /// Plays a pick in the draft. Root checks invariants and then delegates to the appropriate part.
  /// </summary>
  /// <returns>The result</returns>
  public Result PlayPick(
    ISeriesPolicyProvider seriesPolicyProvider,
    DraftPartId draftPartId,
    Movie movie,
    int draftPosition,
    int playOrder,
    ParticipantId participantId)
  {
    ArgumentNullException.ThrowIfNull(draftPartId);
    ArgumentNullException.ThrowIfNull(movie);
    ArgumentNullException.ThrowIfNull(seriesPolicyProvider);

    var part = FindPart(draftPartId);

    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(draftPartId.Value));
    }

    if (part.DraftId != Id)
    {
      return Result.Failure(DraftErrors.DraftPartDoesNotBelongToThisDraft);
    }

    switch (participantId.Kind)
    {
      case 0:
        DrafterId drafterId = participantId.AsDrafterId();

        if (!part.Drafters.Any(d => d.Id == drafterId))
        {
          return Result.Failure(DraftErrors.DrafterDoesNotBelongToThisDraft);
        }
        break;

      case 1:
        var drafterTeamId = participantId.AsDrafterTeamId();
        if (!part.DrafterTeams.Any(dt => dt.Id == drafterTeamId))
        {
          return Result.Failure(DraftErrors.DrafterTeamDoesNotBelongToThisDraft);
        }
        break;

      case 2:
        var budget = ResolvePartBudget(seriesPolicyProvider, part);
        if (budget.MaxCommunityPicks <= 0)
        {
          return Result.Failure(DraftErrors.CommunityPicksNotAllowedInThisDraftPart);
        }

        if (part.CommunityPicksUsed >= budget.MaxCommunityPicks)
        {
          return Result.Failure(DraftErrors.NoRemainingCommunityPicks);
        }
        break;

      default:
        break;
    }

    // Validate that the movie has not already been picked in this draft
    if (IsMovieAlreadyPicked(movie.Id))
    {
      return Result.Failure(DraftErrors.MovieAlreadyPickedInDraft(movie));
    }

    var pick = Pick.Create(
      position: draftPosition,
      movie: movie,
      playedBy: participantId,
      draftPart: part,
      playOrder: playOrder).Value;

    var result = part.AddPick(pick);

    if (result.IsFailure)
    {
      return result;
    }

    Raise(new PickAddedDomainEvent(
      draftId: Id.Value,
      draftPosition,
      movie.Id,
      participantId.IsDrafter ? participantId.AsDrafterId().Value : null,
      participantId.IsTeam ? participantId.AsDrafterTeamId().Value : null,
      playOrder,
      draftPartId: part.Id.Value));

    return Result.Success(result);
  }

  public Result ApplyVeto(
    ISeriesPolicyProvider seriesPolicyProvider,
    DraftPartId draftPartId,
    Pick pick,
    VetoIssuerKind issuerKind,
    ParticipantId issuerId)
  {
    ArgumentNullException.ThrowIfNull(draftPartId);
    ArgumentNullException.ThrowIfNull(seriesPolicyProvider);
    ArgumentNullException.ThrowIfNull(pick);
    ArgumentNullException.ThrowIfNull(issuerKind);

    var part = FindPart(draftPartId);

    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(draftPartId.Value));
    }

    if (issuerKind == VetoIssuerKind.Participant)
    {
      var budget = ResolvePartBudget(seriesPolicyProvider, part);
      var account = GetOrCreateAccount(issuerId);

      if (!account.CanUseVeto(SeriesId!.Value, part.PartIndex, budget.MaxVetoes))
      {
        return Result.Failure(DraftErrors.NoRemainingVetoes);
      }

      account.RecordVetoUse(SeriesId!.Value, part.PartIndex);
    }

    var result = part.RecordVeto(pick.Id, issuerKind, issuerId);

    if (result.IsFailure)
    {
      return result;
    }

    Raise(new VetoAddedDomainEvent(
      draftId: Id.Value,
      participantId: issuerId.Value,
      participantKind: issuerKind.Name,
      pick.Position));

    return Result.Success();
  }

  public Result ApplyVetoOverride(
    ISeriesPolicyProvider seriesPolicyProvider,
    DraftPartId draftPartId,
    Veto veto,
    ParticipantId by)
  {
    ArgumentNullException.ThrowIfNull(draftPartId);
    ArgumentNullException.ThrowIfNull(seriesPolicyProvider);
    ArgumentNullException.ThrowIfNull(veto);

    var part = FindPart(draftPartId);

    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(draftPartId.Value));
    }

    var budget = ResolvePartBudget(seriesPolicyProvider, part);

    if (budget.MaxVetoOverriedes <= 0)
    {
      return Result.Failure(DraftErrors.VetoOverridesNotAllowed);
    }

    var account = GetOrCreateAccount(by);

    if (!account.CanUseVetoOverride(SeriesId!.Value, part.PartIndex, budget.MaxVetoOverriedes))
    {
      return Result.Failure(DraftErrors.NoRemainingVetoOverrides);
    }

    var result = part.RecordVetoOverride(veto, by);

    if (result.IsFailure)
    {
      return result;
    }

    account.RecordVetoOverrideUse(SeriesId!.Value, part.PartIndex);

    Raise(new VetoOverrideAddedDomainEvent(
      draftId: Id.Value,
      participantId: by.Value,
      veto.Id.Value));

    return Result.Success();
  }

  public Result ApplyCommissionerOverride(DraftPartId draftPartId, Pick pick)
  {
    ArgumentNullException.ThrowIfNull(draftPartId);
    ArgumentNullException.ThrowIfNull(pick);

    var part = FindPart(draftPartId);
    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(draftPartId.Value));
    }
    var result = part.ApplyCommissionerOverride(pick);

    if (result.IsFailure)
    {
      return result;
    }

    Raise(new CommissionerOverrideAppliedDomainEvent(
      pick.Id.Value,
      pick.CommissionerOverride.Id));
    return Result.Success();
  }

  private bool IsMovieAlreadyPicked(Guid movieId)
  {
    return _parts.SelectMany(p => p.Picks).Any(pk => pk.MovieId == movieId && !pk.IsVetoed);
  }

  private PartBudget ResolvePartBudget(ISeriesPolicyProvider series, DraftPart part)
  {
    var totalParticipants = part.TotalDrafters + part.TotalDrafterTeams;
    return series.GetPartBudget(SeriesId!, DraftType, part.PartIndex, totalParticipants);
  }

  private DrafterVetoAccount GetOrCreateAccount(ParticipantId participantId)
  {
    if (!_accounts.TryGetValue(participantId, out var account))
    {
      account = new DrafterVetoAccount(participantId);
      _accounts.Add(participantId, account);
    }

    return account;
  }
}

